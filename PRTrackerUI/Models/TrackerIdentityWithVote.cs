using System.Collections.Concurrent;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using PRTrackerUI.Common;

namespace PRTrackerUI.Models
{
    public class TrackerIdentityWithVote : TrackerIdentity
    {
        private readonly IdentityRefWithVote identityRefWithVote;

        public TrackerIdentityWithVote(IdentityRefWithVote identityRefWithVote, AsyncCache<string, BitmapImage> avatarDownloadAsyncCache, ConcurrentDictionary<string, BitmapImage> avatarCache, BitmapImage avatarPlaceholder)
            : base(identityRefWithVote, avatarDownloadAsyncCache, avatarCache, avatarPlaceholder)
        {
            this.identityRefWithVote = identityRefWithVote;
        }

        public Brush Brush
        {
            get
            {
                Brush brush = null;

                if (this.identityRefWithVote.Vote > 0)
                {
                    brush = Brushes.Green;
                }
                else if (this.identityRefWithVote.Vote == -5)
                {
                    brush = Brushes.Orange;
                }
                else if (this.identityRefWithVote.Vote == -10)
                {
                    brush = Brushes.Red;
                }

                return brush;
            }
        }

        public bool IsOverlayVisible { get => this.identityRefWithVote.Vote != 0; }

        public string OverlayText
        {
            get
            {
                string text = string.Empty;

                if (this.identityRefWithVote.Vote > 0)
                {
                    text = "\uea12";
                }
                else if (this.identityRefWithVote.Vote == -5)
                {
                    text = "\uea15";
                }
                else if (this.identityRefWithVote.Vote == -10)
                {
                    text = "\uea04";
                }

                return text;
            }
        }
    }
}
