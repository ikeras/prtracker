namespace PRServicesClient.Contracts
{
    public interface IUserWithVote : IUser
    {
        PullRequestVote Vote { get; }
    }
}