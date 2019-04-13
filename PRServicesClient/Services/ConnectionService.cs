using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace PRServicesClient.Services
{
    public class ConnectionService : IConnectionService
    {
        public async Task<IPullRequestServices> InitializePullRequestServicesAsync(string accountName, string personalAccessToken, string project, string repoId)
        {
            ClientContext clientContext = new ClientContext(accountName, personalAccessToken);

            VssConnection connection = clientContext.Connection;
            GitHttpClient client = connection.GetClient<GitHttpClient>();
            GitRepository repo = await client.GetRepositoryAsync(project, repoId);

            return new AzDOPullRequestServices(clientContext, client, project, repo);
        }
    }
}
