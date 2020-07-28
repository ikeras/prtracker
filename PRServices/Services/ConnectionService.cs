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
        public static async Task<IPullRequestServices> InitializeAzureDevOpsPRServicesAsync(string personalAccessToken, string project, string repoName, string accountName)
        {
            ClientContext clientContext = new ClientContext(accountName, personalAccessToken);

            VssConnection connection = clientContext.Connection;
            GitHttpClient client = connection.GetClient<GitHttpClient>();
            GitRepository repo = await client.GetRepositoryAsync(project, repoName);

            return new AzureDevOpsPullRequestServices(clientContext, client, project, repo);
        }

        public static IPullRequestServices InitializeGitHubPRServicesAsync(string personalAccessToken, string owner, string repoName)
        {
            Credentials credentials = new Credentials(personalAccessToken);

            GitHubClient client = new GitHubClient(new ProductHeaderValue("PRTracker"))
            {
                Credentials = credentials
            };

            return new GitHubPullRequestServices(client, owner, repoName);
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
                case PullRequestProvider.AzureDevOps:
                    return ConnectionService.InitializeAzureDevOpsPRServicesAsync(personalAccessToken, projectOrOwner, repoName, accountName);
                case PullRequestProvider.GitHub:
                    return Task.FromResult(ConnectionService.InitializeGitHubPRServicesAsync(personalAccessToken, projectOrOwner, repoName));
            }

            throw new ArgumentException($"Unknown provider {provider.ToString()}", nameof(provider));
        }
    }
}
