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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Extensions.Configuration;
using PRServices.Contracts;
using PRServices.Services;
using PRTracker.Common;
using PRTracker.Models;
using PRTracker.ViewServices;

namespace PRTracker.ViewModels
{
    public class TrackerTrayIconViewModel : ViewModelBase
    {
        private TrackerConfig config;
        private string iconSource;
        private bool loadEnabled;
        private ObservableCollection<TrackerPullRequest> pullRequests;
        private TrackerPullRequest selectedPullRequest;
        private DispatcherTimer refreshTimer;

        public TrackerTrayIconViewModel()
        {
            this.ExitApplicationCommand = new RelayCommand(this.OnExitApplication);
            this.iconSource = IconSources.Default;
            this.IsUpdating = false;
            this.LaunchReviewToolCommand = new RelayCommand(this.OnLaunchReviewTool);

            this.LoadConfigAndStartRefreshTimer();
        }

        public RelayCommand ExitApplicationCommand { get; }

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

        private INotificationService NotificationService { get => SimpleIoc.Default.GetInstance<INotificationService>(); }

        private void OnLaunchReviewTool()
        {
            // Handle errors
            TrackerPullRequest pullRequest = this.SelectedPullRequest;

            if (pullRequest != null)
            {
                TrackerReviewTool reviewTool = this.config.ReviewTools.FirstOrDefault((tool) => pullRequest.ReviewTool == tool.Name);

                reviewTool.Launch(pullRequest.AccountName, pullRequest.ProjectOrOwner, pullRequest.RepoName, pullRequest.ID, pullRequest.Url);
            }
        }

        private Func<string, Task<BitmapImage>> GetDownloadAvatarImageAsync(Func<string, Task<Stream>> downloadAvatarAsync)
        {
            return async url =>
            {
                try
                {
                    using Stream avatarStream = await downloadAvatarAsync(url);

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

        private void LoadConfigAndStartRefreshTimer()
        {
            this.IsUpdating = true;

            Task.Run(async () =>
            {
                this.config = await this.LoadConfig();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.LoadPullRequests();

                    this.refreshTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMinutes(this.config.UpdateInterval),
                    };
                    this.refreshTimer.Tick += this.OnTimerTick;
                    this.refreshTimer.Start();
                });
            });
        }

        private void LoadPullRequests()
        {
            this.IsUpdating = true;

            Task.Run(async () =>
            {
                IConnectionService connectionService = SimpleIoc.Default.GetInstance<IConnectionService>();
                List<TrackerPullRequest> trackerPullRequests = new List<TrackerPullRequest>();

                ConcurrentDictionary<string, BitmapImage> avatarCache = new ConcurrentDictionary<string, BitmapImage>();

                foreach (TrackerAzureDevOpsQuery query in this.config.AzureDevOps.Queries)
                {
                    IAzureDevOpsPullRequestService adoPRService = connectionService.InitializeAzureDevOpsService(
                        query.PersonalAccessToken,
                        query.Project,
                        query.AccountName);

                    AzureDevOpsQuery adoQuery = new AzureDevOpsQuery
                    {
                        IsAssignedToMe = query.FilterToTeams != null ? true : query.IsAssignedToMe,
                        IsCreatedByMe = query.IsCreatedByMe,
                        FilterToTeams = query.FilterToTeams,
                        RepoName = query.RepoName,
                        SourceRefName = query.SourceRefName,
                        SourceRepoName = query.SourceRepoName,
                        Status = query.Status != null ? Enum.Parse<PullRequestState>(query.Status, true) : PullRequestState.Open,
                        TargetRefName = query.TargetRefName,
                        UniqueUserIdFilter = query.UniqueUserId
                    };

                    IEnumerable<IPullRequest> prs = await adoPRService.GetPullRequestsAsync(adoQuery);
                    AsyncCache<string, BitmapImage> asyncCache = new AsyncCache<string, BitmapImage>(this.GetDownloadAvatarImageAsync(adoPRService.DownloadAvatarAsync));

                    foreach (IPullRequest pullRequest in prs)
                    {
                        trackerPullRequests.Add(new TrackerPullRequest(pullRequest, avatarCache, asyncCache, query.ReviewTool ?? this.config.AzureDevOps.DefaultReviewTool));
                    }
                }

                foreach (TrackerGitHubQuery query in this.config.GitHub.Queries)
                {
                    IGitHubPullRequestService gitHubPRService = connectionService.InitializeGitHubPRServicesAsync(query.PersonalAccessToken);

                    GitHubQuery gitHubQuery = new GitHubQuery
                    {
                        AssginedTo = query.AssginedTo,
                        Base = query.Base,
                        CreatedBy = query.CreatedBy,
                        Head = query.Head,
                        InvolvedUser = query.InvolvedUser,
                        Status = query.Status != null ? Enum.Parse<PullRequestState>(query.Status, true) : PullRequestState.Open,
                        UniqueUserId = query.UniqueUserId
                    };

                    IEnumerable<IPullRequest> prs = await gitHubPRService.GetPullRequestsAsync(gitHubQuery);
                    AsyncCache<string, BitmapImage> asyncCache = new AsyncCache<string, BitmapImage>(this.GetDownloadAvatarImageAsync(gitHubPRService.DownloadAvatarAsync));

                    foreach (IPullRequest pullRequest in prs)
                    {
                        trackerPullRequests.Add(new TrackerPullRequest(pullRequest, avatarCache, asyncCache, query.ReviewTool ?? this.config.GitHub.DefaultReviewTool));
                    }
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // We merge the old and new list together, and if the size is larger, that means there are new PRs to review
                    bool showNotification = this.PullRequests != null ? this.PullRequests.Union(trackerPullRequests, TrackerPullRequestComparer.Default).Count() > this.PullRequests.Count : trackerPullRequests.Count > 0;
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

        private void OnExitApplication()
        {
            Application.Current.Shutdown();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            this.LoadPullRequests();
        }
    }
}