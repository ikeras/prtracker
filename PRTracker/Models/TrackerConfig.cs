namespace PRTracker.Models
{
    public class TrackerConfig
    {
        public double Version { get; set; }

        public TrackerReviewTool[] ReviewTools { get; set; }

        public TrackerAzureDevOps AzureDevOps { get; set; }

        public TrackerGitHub GitHub { get; set; }

        public int UpdateInterval { get; set; }
    }
}
