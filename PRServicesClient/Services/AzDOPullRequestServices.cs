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

        public async Task<IEnumerable<IPullRequest>> GetPullRequestsAsync(PullRequestState status, string userUniqueId = null)
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

            List<IPullRequest> trackerPullRequests = new List<IPullRequest>();

            foreach (GitPullRequest pullRequest in pullRequests)
            {
                DateTime changedStateDate = pullRequest.Status == PullRequestStatus.Completed || pullRequest.Status == PullRequestStatus.Abandoned ? pullRequest.ClosedDate : pullRequest.CreationDate;
                User createdBy = new User(pullRequest.CreatedBy.ImageUrl, pullRequest.CreatedBy.DisplayName);
                IEnumerable<IUserWithVote> reviewers = pullRequest.Reviewers.Select(reviewer => new UserWithVote(reviewer.ImageUrl, reviewer.DisplayName, AzDOPullRequestServices.ConvertToTrackerVote(reviewer.Vote)));

                PullRequest trackerPullRequest = new PullRequest(
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

        private static PullRequestStatus? ConvertToAzDOStatus(PullRequestState status)
        {
            PullRequestStatus? azDOStatus = null;

            switch (status)
            {
                case PullRequestState.All:
                    azDOStatus = PullRequestStatus.All;
                    break;
                case PullRequestState.Closed:
                    azDOStatus = PullRequestStatus.Completed;
                    break;
                case PullRequestState.Open:
                    azDOStatus = PullRequestStatus.Active;
                    break;
            }

            return azDOStatus;
        }

        private static PullRequestState ConvertToTrackerStatus(PullRequestStatus status)
        {
            PullRequestState trackerStatus = PullRequestState.Open;

            switch (status)
            {
                case PullRequestStatus.NotSet:
                case PullRequestStatus.Active:
                    trackerStatus = PullRequestState.Open;
                    break;
                case PullRequestStatus.Abandoned:
                case PullRequestStatus.Completed:
                    trackerStatus = PullRequestState.Closed;
                    break;
                case PullRequestStatus.All:
                    trackerStatus = PullRequestState.All;
                    break;
            }

            return trackerStatus;
        }

        private static PullRequestVote ConvertToTrackerVote(short vote)
        {
            PullRequestVote trackerVote = PullRequestVote.None;

            if (vote > 0)
            {
                trackerVote = PullRequestVote.Approved;
            }
            else if (vote == -5)
            {
                trackerVote = PullRequestVote.ChangesRequested;
            }
            else if (vote == -10)
            {
                trackerVote = PullRequestVote.Rejected;
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