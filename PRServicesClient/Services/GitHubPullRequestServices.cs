using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using PRServicesClient.Contracts;

namespace PRServicesClient.Services
{
    internal class GitHubPullRequestServices : IPullRequestServices
    {
        private readonly GitHubClient client;
        private readonly string repoName;
        private readonly string owner;

        public GitHubPullRequestServices(GitHubClient client, string owner, string repoName)
        {
            this.client = client;
            this.owner = owner;
            this.repoName = repoName;
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

        public async Task<IEnumerable<IPullRequest>> GetPullRequestsAsync(PullRequestState status, string userUniqueId = null)
        {
            PullRequestRequest request = new PullRequestRequest { State = GitHubPullRequestServices.ConvertToGitHubStateFilter(status) };

            IReadOnlyList<PullRequest> pullRequests = await this.client.PullRequest.GetAllForRepository(this.owner, this.repoName, request);

            List<IPullRequest> trackerPullRequests = new List<IPullRequest>();

            foreach (PullRequest pullRequest in pullRequests)
            {
                IReadOnlyList<PullRequestReview> reviews = await this.client.PullRequest.Review.GetAll(this.owner, this.repoName, pullRequest.Number);

                // Want to exclude anything that the user has approved
                if (!string.IsNullOrEmpty(userUniqueId) && reviews.Any(review => review.User.Login == userUniqueId && review.State.Value == PullRequestReviewState.Approved))
                {
                    continue;
                }

                DateTime changeStateDate = pullRequest.State.Value == ItemState.Closed ? pullRequest.ClosedAt.Value.DateTime : pullRequest.UpdatedAt.DateTime;
                Models.User createdBy = new Models.User(pullRequest.User.AvatarUrl, pullRequest.User.Login);
                IEnumerable<IUserWithVote> reviewers = reviews.Select(review => new Models.UserWithVote(review.User.AvatarUrl, review.User.Login, GitHubPullRequestServices.ConvertToTrackerVote(review.State)));

                Models.PullRequest trackerPullRequest = new Models.PullRequest(
                    null,
                    pullRequest.Base.Ref,
                    changeStateDate,
                    createdBy,
                    pullRequest.Number,
                    this.owner,
                    this.repoName,
                    reviewers,
                    pullRequest.State.Value == ItemState.Open ? PullRequestState.Open : PullRequestState.Closed,
                    pullRequest.Title,
                    pullRequest.Url);

                trackerPullRequests.Add(trackerPullRequest);
            }

            return trackerPullRequests;
        }

        private static ItemStateFilter ConvertToGitHubStateFilter(PullRequestState status)
        {
            ItemStateFilter filter = ItemStateFilter.All;

            switch (status)
            {
                case PullRequestState.All:
                    filter = ItemStateFilter.All;
                    break;
                case PullRequestState.Closed:
                    filter = ItemStateFilter.Closed;
                    break;
                case PullRequestState.Open:
                    filter = ItemStateFilter.Open;
                    break;
            }

            return filter;
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
