using System.Collections.Generic;

namespace PRServices.Contracts
{
    public interface IPullRequest
    {
        string AccountName { get; }

        string BaseRef { get; }

        IUser CreatedBy { get; }

        string FormattedDate { get; }

        long ID { get; }

        string ProjectOrOwner { get; }

        string RepoName { get; }

        IEnumerable<IUserWithVote> Reviewers { get; }

        PullRequestState Status { get; }

        string Title { get; }

        string Url { get; }
    }
}
