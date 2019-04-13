using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using PRServicesClient.Contracts;
using PRTrackerUI.Common;

namespace PRTrackerUI.Models
{
    public class TrackerIdentity : ObservableObject
    {
        private readonly AsyncCache<string, BitmapImage> avatarDownloadAsyncCache;
        private readonly ConcurrentDictionary<string, BitmapImage> avatarCache;
        private readonly ITrackerIdentity identity;

        public TrackerIdentity(ITrackerIdentity identity, AsyncCache<string, BitmapImage> avatarDownloadAsyncCache, ConcurrentDictionary<string, BitmapImage> avatarCache)
        {
            this.identity = identity;
            this.avatarDownloadAsyncCache = avatarDownloadAsyncCache;
            this.avatarCache = avatarCache;
        }

        public BitmapImage AvatarImage
        {
            get
            {
                string imageUrl = this.identity.AvatarImageUrl;

                if (this.avatarCache.ContainsKey(imageUrl))
                {
                    return this.avatarCache[imageUrl];
                }
                else
                {
                    Task.Run(async () =>
                    {
                        BitmapImage avatarImage = await this.avatarDownloadAsyncCache[imageUrl];
                        avatarImage.Freeze();

                        // It's fine if this returns false because the key already exists, as we'll just send a property change notification to read reload the value from the cache
                        this.avatarCache.TryAdd(imageUrl, avatarImage);
                        this.RaisePropertyChanged();
                    });

                    return null;
                }
            }
        }

        public string DisplayName { get => this.identity.DisplayName; }
    }
}
