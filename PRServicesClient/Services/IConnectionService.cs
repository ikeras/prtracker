using System.Threading.Tasks;
using PRServicesClient.Contracts;

namespace PRServicesClient.Services
{
    public interface IConnectionService
    {
        Task<IPullRequestServices> InitializePullRequestServicesAsync(PullRequestProvider provider, string personalAccessToken, string projectOrOwner, string repoName, string accountName = null);
    }
}
