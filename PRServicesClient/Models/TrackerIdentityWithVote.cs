using PRServicesClient.Contracts;

namespace PRServicesClient.Models
{
    internal class TrackerIdentityWithVote : TrackerIdentity, ITrackerIdentityWithVote
    {
        public TrackerIdentityWithVote(string avatarImageUrl, string displayName, TrackerVote vote)
            : base(avatarImageUrl, displayName)
        {
            this.Vote = vote;
        }

        public TrackerVote Vote { get; }
    }
}
