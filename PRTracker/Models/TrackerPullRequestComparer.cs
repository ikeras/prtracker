using System.Collections.Generic;

namespace PRTracker.Models
{
    public class TrackerPullRequestComparer : IEqualityComparer<TrackerPullRequest>
    {
        public static TrackerPullRequestComparer Default { get; } = new TrackerPullRequestComparer();

        public bool Equals(TrackerPullRequest x, TrackerPullRequest y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            return x != null && y != null && x.ID == y.ID && x.AccountName == y.AccountName &&
                x.ProjectOrOwner == y.ProjectOrOwner && x.RepoName == y.RepoName;
        }

        public int GetHashCode(TrackerPullRequest obj)
        {
            return obj.Url.GetHashCode();
        }
    }
}
