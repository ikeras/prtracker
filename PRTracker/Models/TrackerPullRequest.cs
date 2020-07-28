using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using PRServices.Contracts;
using PRTracker.Common;

namespace PRTracker.Models
{
    public class TrackerPullRequest
    {
        private readonly AsyncCache<string, BitmapImage> avatarDownloadAsyncCache;
        private readonly ConcurrentDictionary<string, BitmapImage> avatarCache;
        private readonly IPullRequest gitPullRequest;

        public TrackerPullRequest(IPullRequest gitPullRequest, ConcurrentDictionary<string, BitmapImage> avatarCache, AsyncCache<string, BitmapImage> avatarDownloadAsyncCache, string reviewTool)
        {
            this.gitPullRequest = gitPullRequest;
            this.avatarCache = avatarCache;
            this.avatarDownloadAsyncCache = avatarDownloadAsyncCache;
            this.ReviewTool = reviewTool;
        }

        public string AccountName { get => this.gitPullRequest.AccountName; }

        public TrackerIdentity CreatedBy { get => new TrackerIdentity(this.gitPullRequest.CreatedBy, this.avatarDownloadAsyncCache, this.avatarCache); }

        public string FormattedDate { get => this.gitPullRequest.FormattedDate; }

        public long ID { get => this.gitPullRequest.ID; }

        public string ProjectOrOwner { get => this.gitPullRequest.ProjectOrOwner; }

        public string RepoName { get => this.gitPullRequest.RepoName; }

        public string ReviewTool { get; }

        public IEnumerable<TrackerIdentityWithVote> Reviewers { get => this.gitPullRequest.Reviewers.Select((reviewer) => new TrackerIdentityWithVote(reviewer, this.avatarDownloadAsyncCache, this.avatarCache)); }

        public string Status
        {
            get
            {
                string status = string.Empty;

                switch (this.gitPullRequest.Status)
                {
                    case PullRequestState.Closed:
                        status = "Completed";
                        break;
                    case PullRequestState.Open:
                        status = "Active";
                        break;
                }

                return status;
            }
        }

        public string TargetBranchName
        {
            get
            {
                const string refsPrefix = "refs/heads/";

                string target = this.gitPullRequest.BaseRef;
                if (target.StartsWith(refsPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    target = target.Remove(0, refsPrefix.Length);
                }

                return target;
            }
        }

        public string Title { get => this.gitPullRequest.Title; }

        public string Url { get => this.gitPullRequest.Url; }
    }
}
