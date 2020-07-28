namespace PRTracker.Models
{
    public class TrackerAzureDevOpsQuery
    {
        public string AccountName { get; set; }

        public string PersonalAccessToken { get; set; }

        public string Project { get; set; }

        public string RepoName { get; set; }

        public string ReviewTool { get; set; }

        public string UniqueUserId { get; set; }
    }
}
