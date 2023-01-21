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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRServices.Contracts;
using PRServices.Services;
using PRTracker.Common;
using PRTracker.Models;
using PRTracker.ViewServices;

namespace PRTracker.ViewModels
{
    public class TrackerTrayIconViewModel : ObservableObject
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
            set => this.SetProperty(ref this.iconSource, value);
        }

        public bool IsUpdating
        {
            get => this.loadEnabled;
            set => this.SetProperty(ref this.loadEnabled, value);
        }

        public RelayCommand LaunchReviewToolCommand { get; }

        public ObservableCollection<TrackerPullRequest> PullRequests
        {
            get => this.pullRequests;
            set => this.SetProperty(ref this.pullRequests, value);
        }

        public TrackerPullRequest SelectedPullRequest
        {
            get => this.selectedPullRequest;
            set => this.SetProperty(ref this.selectedPullRequest, value);
        }

        private static INotificationService NotificationService { get => App.Current.Services.GetService<INotificationService>(); }

        private static Func<string, Task<BitmapImage>> GetDownloadAvatarImageAsync(Func<string, Task<Stream>> downloadAvatarAsync)
        {
            return async url =>
            {
                try
                {
                    using Stream avatarStream = await downloadAvatarAsync(url);

                    if (avatarStream != null)
                    {
                        BinaryReader reader = new(avatarStream);
                        MemoryStream memoryStream = new();
                        BitmapImage avatarImage = new();

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
                        avatarImage.Freeze();

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

        private static async Task<TrackerConfig> LoadConfig()
        {
            return await Task.Run(() =>
            {
                IConfigurationRoot configuration =
                    new ConfigurationBuilder().
                    AddJsonFile("config.json").
                    Build();

                TrackerConfig trackerConfig = new();
                configuration.Bind(trackerConfig);

                return trackerConfig;
            });
        }

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

        private void LoadConfigAndStartRefreshTimer()
        {
            this.IsUpdating = true;

            Task.Run(async () =>
            {
                this.config = await LoadConfig();

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
                IConnectionService connectionService = App.Current.Services.GetService<IConnectionService>();
                List<TrackerPullRequest> trackerPullRequests = new();

                ConcurrentDictionary<string, BitmapImage> avatarCache = new();

                foreach (TrackerAzureDevOpsQuery query in this.config.AzureDevOps.Queries)
                {
                    IAzureDevOpsPullRequestService adoPRService = connectionService.InitializeAzureDevOpsService(
                        query.PersonalAccessToken,
                        query.Project,
                        query.AccountName);

                    AzureDevOpsQuery adoQuery = new()
                    {
                        IsAssignedToMe = query.FilterToTeams != null || query.IsAssignedToMe,
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
                    AsyncCache<string, BitmapImage> asyncCache = new(GetDownloadAvatarImageAsync(adoPRService.DownloadAvatarAsync));

                    foreach (IPullRequest pullRequest in prs)
                    {
                        trackerPullRequests.Add(new TrackerPullRequest(pullRequest, avatarCache, asyncCache, query.ReviewTool ?? this.config.AzureDevOps.DefaultReviewTool));
                    }
                }

                foreach (TrackerGitHubQuery query in this.config.GitHub.Queries)
                {
                    IGitHubPullRequestService gitHubPRService = connectionService.InitializeGitHubPRServicesAsync(query.PersonalAccessToken);

                    GitHubQuery gitHubQuery = new()
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
                    AsyncCache<string, BitmapImage> asyncCache = new(GetDownloadAvatarImageAsync(gitHubPRService.DownloadAvatarAsync));

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
                        NotificationService.ShowNotification("PRTracker", "New PRs to review", NotificationType.Info);
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