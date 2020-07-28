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
            unchecked
            {
                int hash = 17;

                hash = (hash * 23) + obj.ID.GetHashCode();
                hash = (hash * 23) + obj.ProjectOrOwner.GetHashCode();
                hash = (hash * 23) + obj.RepoName.GetHashCode();

                if (obj.AccountName != null)
                {
                    hash = (hash * 23) + obj.AccountName.GetHashCode();
                }

                return hash;
            }
        }
    }
}
