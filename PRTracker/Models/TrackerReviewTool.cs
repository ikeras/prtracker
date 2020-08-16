using System.Diagnostics;
using System.Text;

namespace PRTracker.Models
{
    public class TrackerReviewTool
    {
        public string Name { get; set; }

        public string CommandLine { get; set; }

        public string Arguments { get; set; }

        public void Launch(string accountName, string projectOrOwner, string repoName, long pullRequestId, string url)
        {
            string commandLine = TrackerReviewTool.FillInPlaceholders(this.CommandLine, accountName, projectOrOwner, repoName, pullRequestId, url);
            string arguments = this.Arguments != null ? TrackerReviewTool.FillInPlaceholders(this.Arguments, accountName, projectOrOwner, repoName, pullRequestId, url) : null;

            using Process browser = new Process();
            browser.StartInfo.UseShellExecute = true;
            browser.StartInfo.FileName = commandLine;
            browser.StartInfo.Arguments = arguments;
            browser.Start();
        }

        private static string FillInPlaceholders(string format, string accountName, string projectOrOwner, string repoName, long pullRequestId, string url)
        {
            StringBuilder commandLineBuilder = new StringBuilder(format);
            commandLineBuilder.Replace("{accountName}", accountName);
            commandLineBuilder.Replace("{project}", projectOrOwner);
            commandLineBuilder.Replace("{owner}", projectOrOwner);
            commandLineBuilder.Replace("{repoName}", repoName);
            commandLineBuilder.Replace("{pullRequestId}", pullRequestId.ToString());
            commandLineBuilder.Replace("{url}", url);

            string commandLine = commandLineBuilder.ToString();
            return commandLine;
        }
    }
}