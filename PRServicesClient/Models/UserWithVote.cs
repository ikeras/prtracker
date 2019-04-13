using PRServicesClient.Contracts;

namespace PRServicesClient.Models
{
    internal class UserWithVote : User, IUserWithVote
    {
        public UserWithVote(string avatarImageUrl, string displayName, PullRequestVote vote)
            : base(avatarImageUrl, displayName)
        {
            this.Vote = vote;
        }

        public PullRequestVote Vote { get; }
    }
}
