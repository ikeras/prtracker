namespace PRServices.Contracts
{
    public interface IUserWithVote : IUser
    {
        PullRequestVote Vote { get; }
    }
}