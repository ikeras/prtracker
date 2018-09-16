using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using PRServicesClient.Services;
using PRTrackerUI.Common;
using PRTrackerUI.Models;
using PRTrackerUI.ViewServices;

namespace PRTrackerUI.ViewModel
{
    public class TrackerTaskTrayIconViewModel : ViewModelBase
    {
        private TrackerConfig config;
        private string iconSource;
        private bool loadEnabled;
        private ObservableCollection<TrackerPullRequest> pullRequests;
        private TrackerPullRequest selectedPullRequest;
        private DispatcherTimer timer;

        public TrackerTaskTrayIconViewModel()
        {
            this.iconSource = IconSources.Default;
            this.IsUpdating = false;
            this.LaunchReviewToolCommand = new RelayCommand(this.OnLaunchReviewTool);
            this.LoadCommand = new RelayCommand(this.OnLoadCommand);
        }

        public string IconSource
        {
            get => this.iconSource;
            set => this.Set(nameof(this.IconSource), ref this.iconSource, value);
        }

        public bool IsUpdating
        {
            get => this.loadEnabled;
            set => this.Set(nameof(this.IsUpdating), ref this.loadEnabled, value);
        }

        public RelayCommand LaunchReviewToolCommand { get; }

        public RelayCommand LoadCommand { get; }

        public ObservableCollection<TrackerPullRequest> PullRequests
        {
            get => this.pullRequests;
            set => this.Set(nameof(this.PullRequests), ref this.pullRequests, value);
        }

        public TrackerPullRequest SelectedPullRequest
        {
            get => this.selectedPullRequest;
            set => this.Set(nameof(this.SelectedPullRequest), ref this.selectedPullRequest, value);
        }

        private INotificationService NotificationService { get => ServiceLocator.Current.GetInstance<INotificationService>(); }

        private void OnLaunchReviewTool()
        {
            // Handle errors
            TrackerPullRequest pullRequest = this.SelectedPullRequest;
            string reviewToolName = pullRequest.Query.ReviewTool ?? this.config.DefaultReviewTool;
            TrackerReviewTool reviewTool = this.config.ReviewTools.FirstOrDefault((tool) => reviewToolName == tool.Name);

            reviewTool.Launch(pullRequest.Query.AccountName, pullRequest.Query.Project, pullRequest.Query.RepoId, pullRequest.ID);
        }

        private Func<string, Task<BitmapImage>> GetDownloadAvatarImageAsync(IPullRequestServices pullRequestServices)
        {
            return async url =>
            {
                try
                {
                    Stream avatarStream = await pullRequestServices.DownloadAvatarAsync(url);

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
                catch (Exception ex)
                {
                    Debug.Fail($"Exception {ex}");
                }

                return null;
            };
        }

        private async Task<TrackerConfig> LoadConfig()
        {
            return await Task.Run(() =>
            {
                IConfigurationRoot configuration =
                    new ConfigurationBuilder().
                    AddJsonFile("config.json").
                    Build();

                TrackerConfig trackerConfig = new TrackerConfig();
                configuration.Bind(trackerConfig);

                return trackerConfig;
            });
        }

        private void LoadPullRequests()
        {
            this.IsUpdating = true;

            Task.Run(async () =>
            {
                IConnectionService connectionService = ServiceLocator.Current.GetInstance<IConnectionService>();
                List<TrackerPullRequest> trackerPullRequests = new List<TrackerPullRequest>();

                ConcurrentDictionary<string, BitmapImage> avatarCache = new ConcurrentDictionary<string, BitmapImage>();

                foreach (TrackerQuery query in this.config.Queries)
                {
                    IPullRequestServices prServices = await connectionService.InitializePullRequestServicesAsync(query.AccountName, query.PersonalAccessToken, query.Project, query.RepoId);

                    IEnumerable<GitPullRequest> prs = await prServices.GetPullRequestsAsync(PullRequestStatus.Active, query.UniqueUserId);
                    AsyncCache<string, BitmapImage> asyncCache = new AsyncCache<string, BitmapImage>(this.GetDownloadAvatarImageAsync(prServices));

                    foreach (GitPullRequest pullRequest in prs)
                    {
                        trackerPullRequests.Add(new TrackerPullRequest(pullRequest, avatarCache, asyncCache, query));
                    }
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // We merge the old and new list together, and if the size is larger, that means there are new PRs to review
                    bool showNotification = this.PullRequests != null ? this.PullRequests.Union(trackerPullRequests, TrackerPullRequestComparer.Default).Count() > this.PullRequests.Count : false;
                    this.PullRequests = new ObservableCollection<TrackerPullRequest>(trackerPullRequests);
                    this.IconSource = trackerPullRequests.Count > 0 ? IconSources.Action : IconSources.Default;
                    this.IsUpdating = false;
                    if (showNotification)
                    {
                        this.NotificationService.ShowNotification("PRTracker", "New PRs to review", NotificationType.Info);
                    }
                });
            });
        }

        private void OnLoadCommand()
        {
            this.IsUpdating = true;

            Task.Run(async () =>
            {
                this.config = await this.LoadConfig();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.LoadPullRequests();

                    this.timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMinutes(this.config.UpdateInterval),
                    };
                    this.timer.Tick += this.OnTimerTick;
                    this.timer.Start();
                });
            });
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            this.LoadPullRequests();
        }
    }
}