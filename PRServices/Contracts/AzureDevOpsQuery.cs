namespace PRServices.Contracts
{
    public class AzureDevOpsQuery
    {
        public string[] FilterToTeams { get; set; }

        public bool IncludeDrafts { get; set; }

        public bool IsAssignedToMe { get; set; }

        public bool IsCreatedByMe { get; set; }

        public string RepoName { get; set; }

        public string Reviewer { get; set; }

        public string SourceRefName { get; set; }

        public string SourceRepoName { get; set; }

        public PullRequestState Status { get; set; }

        public string TargetRefName { get; set; }

        public string UniqueUserIdFilter { get; set; }
    }
}
