using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using PRServices.Contracts;

namespace PRServices.Services
{
    internal class GitHubPullRequestServices : IGitHubPullRequestService
    {
        private readonly GitHubClient client;

        public GitHubPullRequestServices(string personalAccessToken)
        {
            Credentials credentials = new Credentials(personalAccessToken);

            GitHubClient client = new GitHubClient(new ProductHeaderValue("PRTracker"))
            {
                Credentials = credentials
            };

            this.client = client;
        }

        public async Task<Stream> DownloadAvatarAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            IApiResponse<Stream> response = await this.client.Connection.Get<Stream>(new Uri(url), TimeSpan.FromSeconds(30));

            return new MemoryStream((byte[])response.HttpResponse.Body);
        }

        public async Task<IEnumerable<IPullRequest>> GetPullRequestsAsync(GitHubQuery query)
        {
            SearchIssuesRequest search = new SearchIssuesRequest
            {
                Assignee = query.AssginedTo,
                Author = query.CreatedBy,
                Base = query.Base,
                Head = query.Head,
                Involves = query.InvolvedUser,
                Type = IssueTypeQualifier.PullRequest
            };

            if (query.Status != PullRequestState.All)
            {
                search.State = query.Status == PullRequestState.Open ? ItemState.Open : ItemState.Closed;
            }

            SearchIssuesResult searchResult = await this.client.Search.SearchIssues(search);

            List<IPullRequest> trackerPullRequests = new List<IPullRequest>();

            foreach (Issue issue in searchResult.Items)
            {
                PullRequest pullRequest = issue.PullRequest;
                IEnumerable<IUserWithVote> reviewers = null;

                if (issue.State != ItemState.Closed)
                {
                    IReadOnlyList<PullRequestReview> reviews = await this.client.PullRequest.Review.GetAll(issue.Repository.Id, pullRequest.Number);

                    // Want to exclude anything that the user has approved
                    if (!string.IsNullOrEmpty(query.UniqueUserId) && reviews.Any(review => review.User.Login == query.UniqueUserId && review.State.Value == PullRequestReviewState.Approved))
                    {
                        continue;
                    }

                    reviewers = reviews.Select(review => new Models.UserWithVote(review.User.AvatarUrl, review.User.Login, GitHubPullRequestServices.ConvertToTrackerVote(review.State)));
                }

                DateTime changeStateDate;
                if (issue.State == ItemState.Closed && issue.ClosedAt.HasValue)
                {
                    changeStateDate = issue.ClosedAt.Value.DateTime;
                }
                else if (issue.UpdatedAt.HasValue)
                {
                    changeStateDate = issue.UpdatedAt.Value.DateTime;
                }
                else
                {
                    changeStateDate = issue.CreatedAt.DateTime;
                }

                Models.User createdBy = new Models.User(issue.User.AvatarUrl, issue.User.Login);

                Models.PullRequest trackerPullRequest = new Models.PullRequest(
                    null,
                    pullRequest.Base?.Ref,
                    changeStateDate,
                    createdBy,
                    pullRequest.Number,
                    issue.User.Name,
                    issue.Repository?.Name,
                    reviewers,
                    issue.State.Value == ItemState.Open ? PullRequestState.Open : PullRequestState.Closed,
                    issue.Title,
                    issue.HtmlUrl);

                trackerPullRequests.Add(trackerPullRequest);
            }

            return trackerPullRequests;
        }

        private static PullRequestVote ConvertToTrackerVote(StringEnum<PullRequestReviewState> state)
        {
            PullRequestVote trackerVote = PullRequestVote.None;

            switch (state.Value)
            {
                case PullRequestReviewState.Approved:
                    trackerVote = PullRequestVote.Approved;
                    break;
                case PullRequestReviewState.ChangesRequested:
                    trackerVote = PullRequestVote.ChangesRequested;
                    break;
                case PullRequestReviewState.Dismissed:
                    trackerVote = PullRequestVote.Rejected;
                    break;
            }

            return trackerVote;
        }
    }
}
