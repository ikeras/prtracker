using System.Collections.Generic;

namespace PRTrackerUI.Models
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

            return x != null && y != null && x.ID == y.ID && x.Query.AccountName == y.Query.AccountName &&
                x.Query.Project == y.Query.Project && x.Query.RepoId == y.Query.RepoId;
        }

        public int GetHashCode(TrackerPullRequest obj)
        {
            unchecked
            {
                int hash = 17;

                hash = (hash * 23) + obj.ID.GetHashCode();
                hash = (hash * 23) + obj.Query.AccountName.GetHashCode();
                hash = (hash * 23) + obj.Query.Project.GetHashCode();
                hash = (hash * 23) + obj.Query.RepoId.GetHashCode();

                return hash;
            }
        }
    }
}
