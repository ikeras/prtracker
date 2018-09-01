using System.Threading.Tasks;

namespace PRServicesClient.Services
{
    public interface IConnectionService
    {
        Task<IPullRequestServices> InitializePullRequestServicesAsync(string accountName, string personalAccessToken, string project, string repoId);
    }
}
