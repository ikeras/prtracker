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
    internal class AzureDevOpsPullRequestServices : IPullRequestServices
    {
        private readonly ClientContext clientContext;
        private readonly GitHttpClient client;
        private readonly string project;
        private readonly GitRepository repo;

        public AzureDevOpsPullRequestServices(ClientContext clientContext, GitHttpClient client, string project, GitRepository repo)
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
            request.Headers.Add(HttpHeaders.Authorization, "Basic " + AzureDevOpsPullRequestServices.FormatBasicAuthHeader(new NetworkCredential("pat", this.clientContext.PersonalAccessToken)));

            // TODO: put timeout here
            request.Timeout = -1;

            WebResponse response = await request.GetResponseAsync();
            Stream responseStream = response.GetResponseStream();

            return responseStream;
        }

        public async Task<IEnumerable<IPullRequest>> GetPullRequestsAsync(PullRequestState status, string userUniqueId = null)
        {
            IEnumerable<GitPullRequest> pullRequests = await this.client.GetPullRequestsByProjectAsync(this.project, new GitPullRequestSearchCriteria() { RepositoryId = this.repo.Id, Status = AzureDevOpsPullRequestServices.ConvertToAzDOStatus(status) });

            List<IPullRequest> trackerPullRequests = new List<IPullRequest>();

            foreach (GitPullRequest pullRequest in pullRequests)
            {
                // Want to exclude anything that the user has approved
                if (!string.IsNullOrEmpty(userUniqueId) && pullRequest.Reviewers.Any(reviewer => reviewer.UniqueName == userUniqueId && reviewer.Vote > 0))
                {
                    continue;
                }

                DateTime changedStateDate = pullRequest.Status == PullRequestStatus.Completed || pullRequest.Status == PullRequestStatus.Abandoned ? pullRequest.ClosedDate : pullRequest.CreationDate;
                User createdBy = new User(pullRequest.CreatedBy.ImageUrl, pullRequest.CreatedBy.DisplayName);
                IEnumerable<IUserWithVote> reviewers = pullRequest.Reviewers.Select(reviewer => new UserWithVote(reviewer.ImageUrl, reviewer.DisplayName, AzureDevOpsPullRequestServices.ConvertToTrackerVote(reviewer.Vote)));

                PullRequest trackerPullRequest = new PullRequest(
                    this.clientContext.AccountName,
                    pullRequest.TargetRefName,
                    changedStateDate,
                    createdBy,
                    pullRequest.PullRequestId,
                    this.project,
                    this.repo.Name,
                    reviewers,
                    AzureDevOpsPullRequestServices.ConvertToTrackerState(pullRequest.Status),
                    pullRequest.Title,
                    pullRequest.Url);

                trackerPullRequests.Add(trackerPullRequest);
            }

            return trackerPullRequests;
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

        private static PullRequestState ConvertToTrackerState(PullRequestStatus status)
        {
            PullRequestState trackerState = PullRequestState.Open;

            switch (status)
            {
                case PullRequestStatus.NotSet:
                case PullRequestStatus.Active:
                    trackerState = PullRequestState.Open;
                    break;
                case PullRequestStatus.Abandoned:
                case PullRequestStatus.Completed:
                    trackerState = PullRequestState.Closed;
                    break;
                case PullRequestStatus.All:
                    trackerState = PullRequestState.All;
                    break;
            }

            return trackerState;
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