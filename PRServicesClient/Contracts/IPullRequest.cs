using System.Collections.Generic;

namespace PRServicesClient.Contracts
{
    public interface IPullRequest
    {
        IUser CreatedBy { get; }

        string FormattedDate { get; }

        int ID { get; }

        IEnumerable<IUserWithVote> Reviewers { get; }

        PullRequestState Status { get; }

        string BaseRef { get; }

        string Title { get; }

        string Url { get; }
    }
}
