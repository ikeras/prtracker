using System;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Octokit;
using PRServices.Contracts;

namespace PRServices.Services
{
    public class ConnectionService : IConnectionService
    {
        public static IPullRequestServices InitializeGitHubPRServicesAsync(string personalAccessToken, string owner, string repoName)
        {
            Credentials credentials = new Credentials(personalAccessToken);

            GitHubClient client = new GitHubClient(new ProductHeaderValue("PRTracker"))
            {
                Credentials = credentials
            };

            return new GitHubPullRequestServices(client, owner, repoName);
        }

        public IAzureDevOpsPullRequestService InitializeAzureDevOpsService(string personalAccessToken, string project, string accountName)
        {
            return new AzureDevOpsPullRequestServices(personalAccessToken, project, accountName);
        }

        public Task<IPullRequestServices> InitializePullRequestServicesAsync(
            PullRequestProvider provider,
            string personalAccessToken,
            string projectOrOwner,
            string repoName,
            string accountName = null)
        {
            switch (provider)
            {
                case PullRequestProvider.GitHub:
                    return Task.FromResult(ConnectionService.InitializeGitHubPRServicesAsync(personalAccessToken, projectOrOwner, repoName));
            }

            throw new ArgumentException($"Unknown provider {provider.ToString()}", nameof(provider));
        }
    }
}
