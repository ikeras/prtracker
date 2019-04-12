using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using PRServicesClient.Common;
using static System.FormattableString;

namespace PRServicesClient.Services
{
    public class PullRequestServices : IPullRequestServices
    {
        private readonly ClientContext clientContext;
        private readonly GitHttpClient client;
        private readonly string project;
        private readonly GitRepository repo;

        public PullRequestServices(ClientContext clientContext, GitHttpClient client, string project, GitRepository repo)
        {
            this.clientContext = clientContext;
            this.client = client;
            this.project = project;
            this.repo = repo;
        }

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

        public async Task<IEnumerable<GitPullRequest>> GetPullRequestsAsync(PullRequestStatus status, string userUniqueId = null)
        {
            IEnumerable<GitPullRequest> pullRequests = await this.client.GetPullRequestsByProjectAsync(this.project, new GitPullRequestSearchCriteria() { RepositoryId = this.repo.Id, Status = status });

            if (!string.IsNullOrEmpty(userUniqueId))
            {
                pullRequests = pullRequests.Where((pullRequest) =>
                {
                    IdentityRefWithVote identityRefWithVote = pullRequest.Reviewers.FirstOrDefault((identity) => identity.UniqueName == userUniqueId);

                    // Want to include anything that the user hasn't voted for, or where they have voted with waiting or rejected
                    return identityRefWithVote == null || identityRefWithVote.Vote <= 0;
                });
            }

            return pullRequests;
        }

        public async Task<string> GetUrlForBranchRef(string refName)
        {
            List<GitRef> gitRefs = await this.client.GetBranchRefsAsync(this.repo.Id);

            return gitRefs.Where((gitRef) => gitRef.Name == refName).FirstOrDefault()?.Url;
        }

        private static string FormatBasicAuthHeader(NetworkCredential credential)
        {
            string authHeader;

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