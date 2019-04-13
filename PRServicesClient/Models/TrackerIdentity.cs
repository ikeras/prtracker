using PRServicesClient.Contracts;

namespace PRServicesClient.Models
{
    internal class TrackerIdentity : ITrackerIdentity
    {
        public TrackerIdentity(string avatarImageUrl, string displayName)
        {
            this.AvatarImageUrl = avatarImageUrl;
            this.DisplayName = displayName;
        }

        public string AvatarImageUrl { get; }

        public string DisplayName { get; }
    }
}
