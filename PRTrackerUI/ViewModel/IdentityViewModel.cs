using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using Microsoft.VisualStudio.Services.WebApi;
using PRTrackerUI.Common;

namespace PRTrackerUI.ViewModel
{
    public class IdentityViewModel : ObservableObject
    {
        private readonly AsyncCache<string, BitmapImage> avatarDownloadAsyncCache;
        private readonly ConcurrentDictionary<string, BitmapImage> avatarCache;
        private readonly BitmapImage avatarPlaceholder;
        private readonly IdentityRef identityRef;

        public IdentityViewModel(IdentityRef identityRef, AsyncCache<string, BitmapImage> avatarDownloadAsyncCache, ConcurrentDictionary<string, BitmapImage> avatarCache, BitmapImage avatarPlaceholder)
        {
            this.identityRef = identityRef;
            this.avatarDownloadAsyncCache = avatarDownloadAsyncCache;
            this.avatarCache = avatarCache;
            this.avatarPlaceholder = avatarPlaceholder;
        }

        public BitmapImage AvatarImage
        {
            get
            {
                string imageUrl = this.identityRef.ImageUrl;

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

                    return this.avatarPlaceholder;
                }
            }
        }

        public string DisplayName { get => this.identityRef.DisplayName; }
    }
}
