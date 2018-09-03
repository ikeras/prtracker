using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace PRServicesClient.Services
{
    public interface IPullRequestServices
    {
        Task<Stream> DownloadAvatarAsync(string url);

        Task<IEnumerable<GitPullRequest>> GetPullRequestsAsync(PullRequestStatus status, string userUniqueId = null);

        Task<string> GetUrlForBranchRef(string repoId);
    }
}
