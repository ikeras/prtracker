using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using PRServicesClient.Common;
using PRServicesClient.Contracts;
using PRServicesClient.Models;
using static System.FormattableString;

namespace PRServicesClient.Services
{
    public class AzDOPullRequestServices : IPullRequestServices
    {
        private readonly ClientContext clientContext;
        private readonly GitHttpClient client;
        private readonly string project;
        private readonly GitRepository repo;

        public AzDOPullRequestServices(ClientContext clientContext, GitHttpClient client, string project, GitRepository repo)
        {
            this.clientContext = clientContext;
            this.client = client;
            this.project = project;
            this.repo = repo;
        }

        public async Task<Stream> DownloadAvatarAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            WebRequest request = WebRequest.Create(url);
            request.Headers.Add(HttpHeaders.Authorization, "Basic " + AzDOPullRequestServices.FormatBasicAuthHeader(new NetworkCredential("pat", this.clientContext.PersonalAccessToken)));

            // TODO: put timeout here
            request.Timeout = -1;

            WebResponse response = await request.GetResponseAsync();
            Stream responseStream = response.GetResponseStream();

            return responseStream;
        }

        public async Task<IEnumerable<ITrackerPullRequest>> GetPullRequestsAsync(TrackerPullRequestStatus status, string userUniqueId = null)
        {
            IEnumerable<GitPullRequest> pullRequests = await this.client.GetPullRequestsByProjectAsync(this.project, new GitPullRequestSearchCriteria() { RepositoryId = this.repo.Id, Status = AzDOPullRequestServices.ConvertToAzDOStatus(status) });

            if (!string.IsNullOrEmpty(userUniqueId))
            {
                pullRequests = pullRequests.Where((pullRequest) =>
                {
                    IdentityRefWithVote identityRefWithVote = pullRequest.Reviewers.FirstOrDefault((identity) => identity.UniqueName == userUniqueId);

                    // Want to include anything that the user hasn't voted for, or where they have voted with waiting or rejected
                    return identityRefWithVote == null || identityRefWithVote.Vote <= 0;
                });
            }

            List<ITrackerPullRequest> trackerPullRequests = new List<ITrackerPullRequest>();

            foreach (GitPullRequest pullRequest in pullRequests)
            {
                DateTime changedStateDate = pullRequest.Status == PullRequestStatus.Completed || pullRequest.Status == PullRequestStatus.Abandoned ? pullRequest.ClosedDate : pullRequest.CreationDate;
                TrackerIdentity createdBy = new TrackerIdentity(pullRequest.CreatedBy.ImageUrl, pullRequest.CreatedBy.DisplayName);
                IEnumerable<ITrackerIdentityWithVote> reviewers = pullRequest.Reviewers.Select(reviewer => new TrackerIdentityWithVote(reviewer.ImageUrl, reviewer.DisplayName, AzDOPullRequestServices.ConvertToTrackerVote(reviewer.Vote)));

                TrackerPullRequest trackerPullRequest = new TrackerPullRequest(
                    changedStateDate,
                    createdBy,
                    pullRequest.PullRequestId,
                    reviewers,
                    AzDOPullRequestServices.ConvertToTrackerStatus(pullRequest.Status),
                    pullRequest.TargetRefName,
                    pullRequest.Title,
                    pullRequest.Url);

                trackerPullRequests.Add(trackerPullRequest);
            }

            return trackerPullRequests;
        }

        public async Task<string> GetUrlForBranchRef(string refName)
        {
            List<GitRef> gitRefs = await this.client.GetBranchRefsAsync(this.repo.Id);

            return gitRefs.Where((gitRef) => gitRef.Name == refName).FirstOrDefault()?.Url;
        }

        private static PullRequestStatus? ConvertToAzDOStatus(TrackerPullRequestStatus status)
        {
            PullRequestStatus? azDOStatus = null;

            switch (status)
            {
                case TrackerPullRequestStatus.All:
                    azDOStatus = PullRequestStatus.All;
                    break;
                case TrackerPullRequestStatus.Closed:
                    azDOStatus = PullRequestStatus.Completed;
                    break;
                case TrackerPullRequestStatus.Open:
                    azDOStatus = PullRequestStatus.Active;
                    break;
            }

            return azDOStatus;
        }

        private static TrackerPullRequestStatus ConvertToTrackerStatus(PullRequestStatus status)
        {
            TrackerPullRequestStatus trackerStatus = TrackerPullRequestStatus.Open;

            switch (status)
            {
                case PullRequestStatus.NotSet:
                case PullRequestStatus.Active:
                    trackerStatus = TrackerPullRequestStatus.Open;
                    break;
                case PullRequestStatus.Abandoned:
                case PullRequestStatus.Completed:
                    trackerStatus = TrackerPullRequestStatus.Closed;
                    break;
                case PullRequestStatus.All:
                    trackerStatus = TrackerPullRequestStatus.All;
                    break;
            }

            return trackerStatus;
        }

        private static TrackerVote ConvertToTrackerVote(short vote)
        {
            TrackerVote trackerVote = TrackerVote.None;

            if (vote > 0)
            {
                trackerVote = TrackerVote.Approved;
            }
            else if (vote == -5)
            {
                trackerVote = TrackerVote.ChangesRequested;
            }
            else if (vote == -10)
            {
                trackerVote = TrackerVote.Rejected;
            }

            return trackerVote;
        }

        private static string FormatBasicAuthHeader(NetworkCredential credential)
        {
            string authHeader;

            if (!string.IsNullOrEmpty(credential.Domain))
            {
                authHeader = Invariant($"{credential.Domain}\\{credential.UserName}:{credential.Password}");
            }
            else
            {
                authHeader = Invariant($"{credential.UserName}:{credential.Password}");
            }

            return Convert.ToBase64String(VssHttpRequestSettings.Encoding.GetBytes(authHeader));
        }
    }
}