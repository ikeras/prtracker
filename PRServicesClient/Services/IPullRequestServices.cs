using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PRServicesClient.Contracts;

namespace PRServicesClient.Services
{
    public interface IPullRequestServices
    {
        Task<Stream> DownloadAvatarAsync(string url);

        Task<IEnumerable<IPullRequest>> GetPullRequestsAsync(PullRequestState status, string userUniqueId = null);

        Task<string> GetUrlForBranchRef(string repoId);
    }
}
