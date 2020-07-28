using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PRServices.Contracts;

namespace PRServices.Services
{
    public interface IPullRequestServices
    {
        Task<Stream> DownloadAvatarAsync(string url);

        Task<IEnumerable<IPullRequest>> GetPullRequestsAsync(PullRequestState status, string userUniqueId = null);
    }
}
