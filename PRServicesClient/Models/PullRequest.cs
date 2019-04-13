using System;
using System.Collections.Generic;
using PRServicesClient.Contracts;

namespace PRServicesClient.Models
{
    internal class PullRequest : IPullRequest
    {
        private readonly DateTime changedStateDate;

        public PullRequest(
            DateTime changedStateDate,
            IUser createdBy,
            int id,
            IEnumerable<IUserWithVote> reviewers,
            PullRequestState status,
            string baseRef,
            string title,
            string url)
        {
            this.changedStateDate = changedStateDate;
            this.CreatedBy = createdBy;
            this.ID = id;
            this.Reviewers = reviewers;
            this.Status = status;
            this.BaseRef = baseRef;
            this.Title = title;
            this.Url = url;
        }

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

        public int ID { get; }

        public IEnumerable<IUserWithVote> Reviewers { get; }

        public PullRequestState Status { get; }

        public string Title { get; }

        public string Url { get; }
    }
}
