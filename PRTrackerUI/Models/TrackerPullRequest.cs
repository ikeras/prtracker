using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public TrackerIdentity CreatedBy { get => new TrackerIdentity(this.gitPullRequest.CreatedBy, this.avatarDownloadAsyncCache, this.avatarCache, this.avatarPlaceholder); }

        public string FormattedDate
        {
            get
            {
                DateTime changedStateDate = this.gitPullRequest.Status == PullRequestStatus.Completed || this.gitPullRequest.Status == PullRequestStatus.Abandoned ? this.gitPullRequest.ClosedDate : this.gitPullRequest.CreationDate;

                TimeSpan timeSpan = DateTime.Now - changedStateDate;
                double hoursAgo = Math.Round(timeSpan.TotalHours);

                return hoursAgo < 24 ? $"{Math.Abs(hoursAgo)} hours ago" : changedStateDate.ToShortDateString();
            }
        }

        public int ID { get => this.gitPullRequest.PullRequestId; }

        public IEnumerable<TrackerIdentityWithVote> Reviewers { get => this.gitPullRequest.Reviewers.Select((reviewer) => new TrackerIdentityWithVote(reviewer, this.avatarDownloadAsyncCache, this.avatarCache, this.avatarPlaceholder)); }

        public string Status
        {
            get
            {
                string status = string.Empty;

                switch (this.gitPullRequest.Status)
                {
                    case PullRequestStatus.NotSet:
                        break;
                    case PullRequestStatus.Active:
                        status = "Active";
                        break;
                    case PullRequestStatus.Abandoned:
                        status = "Abandoned";
                        break;
                    case PullRequestStatus.Completed:
                        status = "Completed";
                        break;
                    case PullRequestStatus.All:
                        break;
                }

                return status;
            }
        }

        public string TargetBranchName
        {
            get
            {
                const string refsPrefix = "refs/heads/";

                string target = this.gitPullRequest.TargetRefName;
                if (target.StartsWith(refsPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    target = target.Remove(0, refsPrefix.Length);
                }

                return target;
            }
        }

        public string Title { get => this.gitPullRequest.Title; }

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

                        const int BytesToRead = 8192;

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
