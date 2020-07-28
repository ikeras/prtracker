using System;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace PRServices.Services
{
    public class ClientContext
    {
        private static readonly string AccountUrlPattern = "https://{0}.visualstudio.com";

        private VssConnection connection;

        public ClientContext(string accountName, string personalAccessToken)
        {
            if (string.IsNullOrEmpty(accountName))
            {
                throw new ArgumentNullException(nameof(accountName));
            }

            if (string.IsNullOrEmpty(personalAccessToken))
            {
                throw new ArgumentNullException(nameof(personalAccessToken));
            }

            this.AccountName = accountName;
            this.Url = new Uri(string.Format(AccountUrlPattern, accountName));
            this.PersonalAccessToken = personalAccessToken;
            this.Credentials = new VssBasicCredential("pat", personalAccessToken);
        }

        public string AccountName { get; }

        public VssCredentials Credentials { get; }

        public string PersonalAccessToken { get; }

        public Uri Url { get; }

        public VssConnection Connection
        {
            get
            {
                if (this.connection == null)
                {
                    this.connection = new VssConnection(this.Url, this.Credentials);
                }

                return this.connection;
            }

            private set => this.connection = value;
        }
    }
}