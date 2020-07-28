namespace PRTracker.Models
{
    public class TrackerGitHubQuery
    {
        public string PersonalAccessToken { get; set; }

        public string Owner { get; set; }

        public string RepoName { get; set; }

        public string ReviewTool { get; set; }

        public string UniqueUserId { get; set; }
    }
}
