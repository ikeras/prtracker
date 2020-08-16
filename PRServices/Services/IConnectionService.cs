namespace PRServices.Services
{
    public interface IConnectionService
    {
        IAzureDevOpsPullRequestService InitializeAzureDevOpsService(string personalAccessToken, string project, string accountName);

        IGitHubPullRequestService InitializeGitHubPRServicesAsync(string personalAccessToken);
    }
}
