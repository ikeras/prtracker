namespace PRServicesClient.Contracts
{
    public interface ITrackerIdentityWithVote : ITrackerIdentity
    {
        TrackerVote Vote { get; }
    }
}