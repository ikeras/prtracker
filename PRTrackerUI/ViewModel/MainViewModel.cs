using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using PRServicesClient.Services;
using PRTrackerUI.Models;

namespace PRTrackerUI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<TrackerPullRequest> pullRequests;
        private bool loadEnabled;

        public MainViewModel()
        {
            this.IsLoadEnabled = true;
            this.LoadCommand = new RelayCommand(this.OnLoadCommand);
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

        private void OnLoadCommand()
        {
            this.IsLoadEnabled = false;

            Task.Run(async () =>
            {
                IConnectionService connectionService = SimpleIoc.Default.GetInstance<IConnectionService>();

                IPullRequestServices prServices = connectionService.InitializePullRequestServices("devdiv", "j7sjpmvftovr7ac5mhslbvcpeopniazsz7e6zodzk2jbanpgjojq");

                List<GitPullRequest> prs = await prServices.GetPullRequestsAsync("devdiv", "VSCloudKernel", PullRequestStatus.Completed);
                List<TrackerPullRequest> trackerPullRequests = new List<TrackerPullRequest>();

                foreach (GitPullRequest pullRequest in prs)
                {
                    trackerPullRequests.Add(new TrackerPullRequest(pullRequest, prServices));
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.PullRequests = new ObservableCollection<TrackerPullRequest>(trackerPullRequests);
                    this.IsLoadEnabled = true;
                });
            });
        }
    }
}