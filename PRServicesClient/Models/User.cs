using PRServicesClient.Contracts;

namespace PRServicesClient.Models
{
    internal class User : IUser
    {
        public User(string avatarImageUrl, string displayName)
        {
            this.AvatarImageUrl = avatarImageUrl;
            this.DisplayName = displayName;
        }

        public string AvatarImageUrl { get; }

        public string DisplayName { get; }
    }
}
