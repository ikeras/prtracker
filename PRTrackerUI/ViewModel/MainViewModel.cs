using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
        private ObservableCollection<TrackerPullRequest> pullRequests;
        private bool loadEnabled;
        private string iconSource = IconSources.Default;

        public MainViewModel()
        {
            this.IsLoadEnabled = true;
            this.LoadCommand = new RelayCommand(this.OnLoadCommand);
        }

        public string IconSource
        {
            get => this.iconSource;
            set => this.Set(nameof(this.IconSource), ref this.iconSource, value);
        }

        public RelayCommand LoadCommand { get; }

        public bool IsLoadEnabled
        {
            get => this.loadEnabled;
            set => this.Set(nameof(this.IsLoadEnabled), ref this.loadEnabled, value);
        }

        public ObservableCollection<TrackerPullRequest> PullRequests
        {
            get => this.pullRequests;
            set => this.Set(nameof(this.PullRequests), ref this.pullRequests, value);
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
                TrackerConfig trackerConfig = this.LoadConfig();

                IConnectionService connectionService = SimpleIoc.Default.GetInstance<IConnectionService>();
                List<TrackerPullRequest> trackerPullRequests = new List<TrackerPullRequest>();

                ConcurrentDictionary<string, BitmapImage> avatarCache = new ConcurrentDictionary<string, BitmapImage>();

                foreach (TrackerQuery query in trackerConfig.Queries)
                {
                    IPullRequestServices prServices = await connectionService.InitializePullRequestServicesAsync(query.AccountName, query.PersonalAccessToken, query.Project, query.RepoId);

                    List<GitPullRequest> prs = await prServices.GetPullRequestsAsync(PullRequestStatus.Completed);
                    AsyncCache<string, BitmapImage> asyncCache = new AsyncCache<string, BitmapImage>(this.GetDownloadAvatarImageAsync(prServices));
                    prs.ForEach((pullRequest) => trackerPullRequests.Add(new TrackerPullRequest(pullRequest, avatarCache, asyncCache)));
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.PullRequests = new ObservableCollection<TrackerPullRequest>(trackerPullRequests);
                    this.IconSource = trackerPullRequests.Count > 0 ? IconSources.Action : IconSources.Default;
                    this.IsLoadEnabled = true;
                });
            });
        }
    }
}