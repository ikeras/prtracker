using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using PRServicesClient.Common;
using static System.FormattableString;

namespace PRServicesClient.Services
{
    public class PullRequestServices : IConnectionService, IPullRequestServices
    {
        private ClientContext clientContext;

        public async Task<Stream> DownloadAvatarAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            WebRequest request = WebRequest.Create(url);
            request.Headers.Add(HttpHeaders.Authorization, "Basic " + PullRequestServices.FormatBasicAuthHeader(new NetworkCredential("pat", this.clientContext.PersonalAccessToken)));

            // TODO: put timeout here
            request.Timeout = -1;

            WebResponse response = await request.GetResponseAsync();
            Stream responseStream = response.GetResponseStream();

            return responseStream;
        }

        public async Task<List<GitPullRequest>> GetPullRequestsAsync(string project, string repoId, PullRequestStatus status)
        {
            VssConnection connection = this.clientContext.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();
            GitRepository repo = await gitClient.GetRepositoryAsync(project, repoId);

            List<GitPullRequest> pullRequests = await gitClient.GetPullRequestsByProjectAsync(project, new GitPullRequestSearchCriteria() { RepositoryId = repo.Id, Status = status });

            return pullRequests;
        }

        public IPullRequestServices InitializePullRequestServices(string accountName, string personalAccessToken)
        {
            this.clientContext = new ClientContext(accountName, personalAccessToken);

            return this;
        }

        private static string FormatBasicAuthHeader(NetworkCredential credential)
        {
            string authHeader = string.Empty;

            if (!string.IsNullOrEmpty(credential.Domain))
            {
                authHeader = Invariant($"{credential.Domain}\\{credential.UserName}:{credential.Password}");
            }
            else
            {
                authHeader = Invariant($"{credential.UserName}:{credential.Password}");
            }

            return Convert.ToBase64String(VssHttpRequestSettings.Encoding.GetBytes(authHeader));
        }
    }
}