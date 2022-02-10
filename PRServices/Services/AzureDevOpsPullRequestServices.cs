using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Account;
using Microsoft.VisualStudio.Services.Account.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.UserMapping;
using Microsoft.VisualStudio.Services.UserMapping.Client;
using Microsoft.VisualStudio.Services.WebApi;
using PRServices.Common;
using PRServices.Contracts;
using PRServices.Models;
using static System.FormattableString;

namespace PRServices.Services
{
    internal class AzureDevOpsPullRequestServices : IAzureDevOpsPullRequestService
    {
        private readonly ClientContext clientContext;
        private readonly VssConnection connection;
        private readonly TeamHttpClient teamClient;
        private readonly GitHttpClient gitClient;
        private readonly string project;

        public AzureDevOpsPullRequestServices(string personalAccessToken, string project, string accountName)
        {
            this.clientContext = new ClientContext(accountName, personalAccessToken);

            this.connection = this.clientContext.Connection;
            this.teamClient = this.connection.GetClient<TeamHttpClient>();
            this.gitClient = this.connection.GetClient<GitHttpClient>();
            this.project = project;
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

        public async Task<IEnumerable<IPullRequest>> GetPullRequestsAsync(AzureDevOpsQuery query)
        {
            List<GitPullRequestSearchCriteria> searches = await this.ConvertQueryToSearches(query);

            List<GitPullRequest> pullRequests = new List<GitPullRequest>();

            foreach (GitPullRequestSearchCriteria search in searches)
            {
                IEnumerable<GitPullRequest> searchResults = await this.gitClient.GetPullRequestsByProjectAsync(this.project, search);
                pullRequests.AddRange(searchResults);
            }

            List<IPullRequest> trackerPullRequests = new List<IPullRequest>();

            foreach (GitPullRequest pullRequest in pullRequests)
            {
                // Want to exclude anything that the user has approved or exclude drafts if not requested
                if ((!string.IsNullOrEmpty(query.UniqueUserIdFilter) && pullRequest.Reviewers.Any(reviewer => reviewer.UniqueName == query.UniqueUserIdFilter && reviewer.Vote > 0)) ||
                    (!query.IncludeDrafts && pullRequest.IsDraft.HasValue && pullRequest.IsDraft.Value))
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
                    pullRequest.Repository.Name,
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

        private async Task<List<GitPullRequestSearchCriteria>> ConvertQueryToSearches(AzureDevOpsQuery query)
        {
            List<GitPullRequestSearchCriteria> searches = new List<GitPullRequestSearchCriteria>();

            if (query.IsAssignedToMe)
            {
                List<WebApiTeam> teams = await this.teamClient.GetAllTeamsAsync(true);

                if (query.FilterToTeams != null)
                {
                    teams = teams.Where(team => query.FilterToTeams.Contains(team.Name)).ToList();
                }

                foreach (WebApiTeam team in teams)
                {
                    GitPullRequestSearchCriteria teamSearchCriteria = new GitPullRequestSearchCriteria
                    {
                        CreatorId = query.IsCreatedByMe ? this.connection.AuthorizedIdentity.Id : (Guid?)null,
                        RepositoryId = await this.RepoNameToRepoId(query.RepoName),
                        ReviewerId = team.Id,
                        SourceRefName = query.SourceRefName,
                        SourceRepositoryId = await this.RepoNameToRepoId(query.SourceRepoName),
                        Status = AzureDevOpsPullRequestServices.ConvertToAzDOStatus(query.Status),
                        TargetRefName = query.TargetRefName
                    };

                    searches.Add(teamSearchCriteria);
                }
            }

            GitPullRequestSearchCriteria searchCriteria = new GitPullRequestSearchCriteria
            {
                CreatorId = query.IsCreatedByMe ? this.connection.AuthorizedIdentity.Id : (Guid?)null,
                RepositoryId = await this.RepoNameToRepoId(query.RepoName),
                ReviewerId = query.IsAssignedToMe ? this.connection.AuthorizedIdentity.Id : (Guid?)null,
                SourceRefName = query.SourceRefName,
                SourceRepositoryId = await this.RepoNameToRepoId(query.SourceRepoName),
                Status = AzureDevOpsPullRequestServices.ConvertToAzDOStatus(query.Status),
                TargetRefName = query.TargetRefName
            };

            searches.Add(searchCriteria);

            return searches;
        }

        private async Task<Guid?> RepoNameToRepoId(string repoName)
        {
            Guid? repositoryId = null;

            if (!string.IsNullOrWhiteSpace(repoName))
            {
                GitRepository repo = await this.gitClient.GetRepositoryAsync(this.project, repoName);
                repositoryId = repo.Id;
            }

            return repositoryId;
        }
    }
}