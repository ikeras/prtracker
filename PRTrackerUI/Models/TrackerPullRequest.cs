using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using PRServicesClient.Services;
using PRTrackerUI.Common;

namespace PRTrackerUI.Models
{
    public class TrackerPullRequest : ObservableObject
    {
        private readonly AsyncCache<string, BitmapImage> avatarDownloadAsyncCache;
        private readonly ConcurrentDictionary<string, BitmapImage> avatarCache;
        private readonly IPullRequestServices pullRequestServices;
        private GitPullRequest gitPullRequest;
        private BitmapImage avatarPlaceholder;

        public TrackerPullRequest(GitPullRequest gitPullRequest, IPullRequestServices pullRequestServices)
        {
            this.gitPullRequest = gitPullRequest;
            this.pullRequestServices = pullRequestServices;
            this.avatarCache = new ConcurrentDictionary<string, BitmapImage>();
            this.avatarDownloadAsyncCache = new AsyncCache<string, BitmapImage>(this.DownloadAvatarImageAsync);
            this.avatarPlaceholder = new BitmapImage(new Uri("pack://application:,,,/Images/placeholder.png", UriKind.Absolute));
            this.avatarPlaceholder.Freeze();
        }

        public string Title { get => this.gitPullRequest.Title; }

        public BitmapImage AvatarImage
        {
            get
            {
                string imageUrl = this.gitPullRequest.CreatedBy.ImageUrl;

                if (this.avatarCache.ContainsKey(imageUrl))
                {
                    return this.avatarCache[imageUrl];
                }
                else
                {
                    Task.Run(async () =>
                    {
                        // It's fine if this returns false because the key already exists, as we'll just send a property change notification to read reload the value from the cache
                        BitmapImage avatarImage = await this.avatarDownloadAsyncCache[imageUrl];
                        avatarImage.Freeze();
                        this.avatarCache.TryAdd(imageUrl, avatarImage);
                        this.RaisePropertyChanged();
                    });

                    return this.avatarPlaceholder;
                }
            }
        }

        private async Task<BitmapImage> DownloadAvatarImageAsync(string url)
        {
            try
            {
                Stream avatarStream = await this.pullRequestServices.DownloadAvatarAsync(url);
                {
                    if (avatarStream != null)
                    {
                        BinaryReader reader = new BinaryReader(avatarStream);
                        MemoryStream memoryStream = new MemoryStream();
                        BitmapImage avatarImage = new BitmapImage();

                        const int BytesToRead = 1000;

                        byte[] bytebuffer = new byte[BytesToRead];
                        int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

                        while (bytesRead > 0)
                        {
                            memoryStream.Write(bytebuffer, 0, bytesRead);
                            bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
                        }

                        avatarImage.BeginInit();
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        avatarImage.StreamSource = memoryStream;
                        avatarImage.EndInit();

                        return avatarImage;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail($"Exception {ex}");
            }

            return null;
        }
    }
}
