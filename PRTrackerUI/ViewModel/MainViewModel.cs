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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using PRServicesClient.Services;
using PRTrackerUI.Common;
using PRTrackerUI.Models;

namespace PRTrackerUI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private TrackerConfig config;
        private string iconSource = IconSources.Default;
        private bool loadEnabled;
        private ObservableCollection<PullRequestViewModel> pullRequests;

        public MainViewModel()
        {
            this.IsLoadEnabled = true;
            this.LaunchReviewToolCommand = new RelayCommand(this.OnLaunchReviewTool);
            this.LoadCommand = new RelayCommand(this.OnLoadCommand);
        }

        public string IconSource
        {
            get => this.iconSource;
            set => this.Set(nameof(this.IconSource), ref this.iconSource, value);
        }

        public bool IsLoadEnabled
        {
            get => this.loadEnabled;
            set => this.Set(nameof(this.IsLoadEnabled), ref this.loadEnabled, value);
        }

        public RelayCommand LaunchReviewToolCommand { get; }

        public RelayCommand LoadCommand { get; }

        public ObservableCollection<PullRequestViewModel> PullRequests
        {
            get => this.pullRequests;
            set => this.Set(nameof(this.PullRequests), ref this.pullRequests, value);
        }

        public PullRequestViewModel SelectedPullRequest { get; set; }

        private void OnLaunchReviewTool()
        {
            // Handle errors
            PullRequestViewModel pullRequest = this.SelectedPullRequest;
            string reviewToolName = pullRequest.Query.ReviewTool ?? this.config.DefaultReviewTool;
            TrackerReviewTool reviewTool = this.config.ReviewTools.FirstOrDefault((tool) => reviewToolName == tool.Name);

            reviewTool.Launch(pullRequest.Query.AccountName, pullRequest.Query.Project, pullRequest.Query.RepoId, pullRequest.ID);
        }

        private TrackerConfig LoadConfig()
        {
            IConfigurationRoot configuration =
                new ConfigurationBuilder().
                AddJsonFile("config.json").
                Build();

            TrackerConfig trackerConfig = new TrackerConfig();
            configuration.Bind(trackerConfig);

            return trackerConfig;
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

        private void OnLoadCommand()
        {
            this.IsLoadEnabled = false;

            Task.Run(async () =>
            {
                if (this.config == null)
                {
                    this.config = this.LoadConfig();
                }

                IConnectionService connectionService = SimpleIoc.Default.GetInstance<IConnectionService>();
                List<PullRequestViewModel> trackerPullRequests = new List<PullRequestViewModel>();

                ConcurrentDictionary<string, BitmapImage> avatarCache = new ConcurrentDictionary<string, BitmapImage>();

                foreach (TrackerQuery query in this.config.Queries)
                {
                    IPullRequestServices prServices = await connectionService.InitializePullRequestServicesAsync(query.AccountName, query.PersonalAccessToken, query.Project, query.RepoId);

                    IEnumerable<GitPullRequest> prs = await prServices.GetPullRequestsAsync(PullRequestStatus.Active, query.UniqueUserId);
                    AsyncCache<string, BitmapImage> asyncCache = new AsyncCache<string, BitmapImage>(this.GetDownloadAvatarImageAsync(prServices));

                    foreach (GitPullRequest pullRequest in prs)
                    {
                        trackerPullRequests.Add(new PullRequestViewModel(pullRequest, avatarCache, asyncCache, query));
                    }
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.PullRequests = new ObservableCollection<PullRequestViewModel>(trackerPullRequests);
                    this.IconSource = trackerPullRequests.Count > 0 ? IconSources.Action : IconSources.Default;
                    this.IsLoadEnabled = true;
                });
            });
        }
    }
}