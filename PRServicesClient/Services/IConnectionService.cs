namespace PRServicesClient.Services
{
    public interface IConnectionService
    {
        IPullRequestServices InitializePullRequestServices(string accountName, string personalAccessToken);
    }
}
