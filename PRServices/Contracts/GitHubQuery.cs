using System;
using System.Collections.Generic;
using System.Text;

namespace PRServices.Contracts
{
    public class GitHubQuery
    {
        public string AssginedTo { get; set; }

        public string Base { get; set; }

        public string CreatedBy { get; set; }

        public string Head { get; set; }

        public string InvolvedUser { get; set; }

        public PullRequestState Status { get; set; }

        public string UniqueUserId { get; set; }
    }
}
