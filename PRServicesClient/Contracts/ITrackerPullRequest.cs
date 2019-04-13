using System.Collections.Generic;

namespace PRServicesClient.Contracts
{
    public interface ITrackerPullRequest
    {
        ITrackerIdentity CreatedBy { get; }

        string FormattedDate { get; }

        int ID { get; }

        IEnumerable<ITrackerIdentityWithVote> Reviewers { get; }

        TrackerPullRequestStatus Status { get; }

        string BaseRef { get; }

        string Title { get; }

        string Url { get; }
    }
}
