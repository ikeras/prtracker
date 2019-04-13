using System.Collections.Concurrent;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PRServicesClient.Contracts;
using PRTrackerUI.Common;

namespace PRTrackerUI.Models
{
    public class TrackerIdentityWithVote : TrackerIdentity
    {
        private readonly IUserWithVote identityWithVote;

        public TrackerIdentityWithVote(IUserWithVote identityWithVote, AsyncCache<string, BitmapImage> avatarDownloadAsyncCache, ConcurrentDictionary<string, BitmapImage> avatarCache)
            : base(identityWithVote, avatarDownloadAsyncCache, avatarCache)
        {
            this.identityWithVote = identityWithVote;
        }

        public Brush Brush
        {
            get
            {
                Brush brush = null;

                switch (this.identityWithVote.Vote)
                {
                    case PullRequestVote.Approved:
                        brush = Brushes.Green;
                        break;
                    case PullRequestVote.ChangesRequested:
                        brush = Brushes.Orange;
                        break;
                    case PullRequestVote.Rejected:
                        brush = Brushes.Red;
                        break;
                }

                return brush;
            }
        }

        public bool IsOverlayVisible { get => this.identityWithVote.Vote != PullRequestVote.None; }

        public string OverlayText
        {
            get
            {
                string text = string.Empty;

                switch (this.identityWithVote.Vote)
                {
                    case PullRequestVote.Approved:
                        text = "\uea12";
                        break;
                    case PullRequestVote.ChangesRequested:
                        text = "\uea15";
                        break;
                    case PullRequestVote.Rejected:
                        text = "\uea04";
                        break;
                }

                return text;
            }
        }
    }
}
