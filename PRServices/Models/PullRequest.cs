using System;
using System.Collections.Generic;
using PRServices.Contracts;

namespace PRServices.Models
{
    internal class PullRequest : IPullRequest
    {
        private readonly DateTime changedStateDate;

        public PullRequest(
            string accountName,
            string baseRef,
            DateTime changedStateDate,
            IUser createdBy,
            long id,
            string projectOrOwner,
            string repoName,
            IEnumerable<IUserWithVote> reviewers,
            PullRequestState status,
            string title,
            string url)
        {
            this.AccountName = accountName;
            this.BaseRef = baseRef;
            this.changedStateDate = changedStateDate;
            this.CreatedBy = createdBy;
            this.ID = id;
            this.ProjectOrOwner = projectOrOwner;
            this.RepoName = repoName;
            this.Reviewers = reviewers;
            this.Status = status;
            this.Title = title;
            this.Url = url;
        }

        public string AccountName { get; }

        public string BaseRef { get; }

        public IUser CreatedBy { get; }

        public string FormattedDate
        {
            get
            {
                TimeSpan timeSpan = DateTime.Now - this.changedStateDate;
                double hoursAgo = Math.Round(timeSpan.TotalHours);

                return hoursAgo < 24 ? $"{Math.Abs(hoursAgo)} hours ago" : this.changedStateDate.ToShortDateString();
            }
        }

        public long ID { get; }

        public string ProjectOrOwner { get; }

        public string RepoName { get; }

        public IEnumerable<IUserWithVote> Reviewers { get; }

        public PullRequestState Status { get; }

        public string Title { get; }

        public string Url { get; }
    }
}
