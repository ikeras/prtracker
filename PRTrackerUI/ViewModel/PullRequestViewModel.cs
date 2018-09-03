﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using PRTrackerUI.Common;

namespace PRTrackerUI.ViewModel
{
    public class PullRequestViewModel : ObservableObject
    {
        private readonly AsyncCache<string, BitmapImage> avatarDownloadAsyncCache;
        private readonly ConcurrentDictionary<string, BitmapImage> avatarCache;
        private readonly GitPullRequest gitPullRequest;
        private readonly BitmapImage avatarPlaceholder;

        public PullRequestViewModel(GitPullRequest gitPullRequest, ConcurrentDictionary<string, BitmapImage> avatarCache, AsyncCache<string, BitmapImage> avatarDownloadAsyncCache)
        {
            this.gitPullRequest = gitPullRequest;
            this.avatarCache = avatarCache;
            this.avatarDownloadAsyncCache = avatarDownloadAsyncCache;
            this.avatarPlaceholder = new BitmapImage(new Uri("pack://application:,,,/Images/placeholder.png", UriKind.Absolute));
            this.avatarPlaceholder.Freeze();
        }

        public RelayCommand ClickCommand { get; }

        public IdentityViewModel CreatedBy { get => new IdentityViewModel(this.gitPullRequest.CreatedBy, this.avatarDownloadAsyncCache, this.avatarCache, this.avatarPlaceholder); }

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

        public IEnumerable<IdentityWithVoteViewModel> Reviewers { get => this.gitPullRequest.Reviewers.Select((reviewer) => new IdentityWithVoteViewModel(reviewer, this.avatarDownloadAsyncCache, this.avatarCache, this.avatarPlaceholder)); }

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

        public string Url { get => this.gitPullRequest.Url; }
    }
}