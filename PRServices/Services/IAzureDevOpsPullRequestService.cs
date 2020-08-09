using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PRServices.Contracts;

namespace PRServices.Services
{
    public interface IAzureDevOpsPullRequestService
    {
        Task<Stream> DownloadAvatarAsync(string url);

        Task<IEnumerable<IPullRequest>> GetPullRequestsAsync(AzureDevOpsQuery query);
    }
}
