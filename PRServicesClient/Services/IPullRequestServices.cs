using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace PRServicesClient.Services
{
    public interface IPullRequestServices
    {
        Task<Stream> DownloadAvatarAsync(string url);

        Task<List<GitPullRequest>> GetPullRequestsAsync(string project, string repoId, PullRequestStatus status);
    }
}
