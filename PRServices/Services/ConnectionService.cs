namespace PRServices.Services
{
    public class ConnectionService : IConnectionService
    {
        public IGitHubPullRequestService InitializeGitHubPRServicesAsync(string personalAccessToken)
        {
            return new GitHubPullRequestServices(personalAccessToken);
        }

        public IAzureDevOpsPullRequestService InitializeAzureDevOpsService(string personalAccessToken, string project, string accountName)
        {
            return new AzureDevOpsPullRequestServices(personalAccessToken, project, accountName);
        }
    }
}
