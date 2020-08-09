using System.Threading.Tasks;
using PRServices.Contracts;

namespace PRServices.Services
{
    public interface IConnectionService
    {
        IAzureDevOpsPullRequestService InitializeAzureDevOpsService(string personalAccessToken, string project, string accountName);

        Task<IPullRequestServices> InitializePullRequestServicesAsync(PullRequestProvider provider, string personalAccessToken, string projectOrOwner, string repoName, string accountName = null);
    }
}
