using System.Collections.Concurrent;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PRServicesClient.Contracts;
using PRTrackerUI.Common;

namespace PRTrackerUI.Models
{
    public class TrackerIdentityWithVote : TrackerIdentity
    {
        private readonly ITrackerIdentityWithVote identityWithVote;

        public TrackerIdentityWithVote(ITrackerIdentityWithVote identityWithVote, AsyncCache<string, BitmapImage> avatarDownloadAsyncCache, ConcurrentDictionary<string, BitmapImage> avatarCache)
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
                    case TrackerVote.Approved:
                        brush = Brushes.Green;
                        break;
                    case TrackerVote.ChangesRequested:
                        brush = Brushes.Orange;
                        break;
                    case TrackerVote.Rejected:
                        brush = Brushes.Red;
                        break;
                }

                return brush;
            }
        }

        public bool IsOverlayVisible { get => this.identityWithVote.Vote != TrackerVote.None; }

        public string OverlayText
        {
            get
            {
                string text = string.Empty;

                switch (this.identityWithVote.Vote)
                {
                    case TrackerVote.Approved:
                        text = "\uea12";
                        break;
                    case TrackerVote.ChangesRequested:
                        text = "\uea15";
                        break;
                    case TrackerVote.Rejected:
                        text = "\uea04";
                        break;
                }

                return text;
            }
        }
    }
}
