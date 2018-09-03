using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace PRTrackerUI.Models
{
    public class TrackerReviewTool
    {
        public string Name { get; set; }

        public string CommandLine { get; set; }

        public string Arguments { get; set; }

        public void Launch(string accountName, string project, string repoId, int pullRequestId)
        {
            string commandLine = TrackerReviewTool.FillInPlaceholders(this.CommandLine, accountName, project, repoId, pullRequestId);
            string arguments = this.Arguments != null ? TrackerReviewTool.FillInPlaceholders(this.Arguments, accountName, project, repoId, pullRequestId) : null;

            Process.Start(commandLine, arguments);
        }

        private static string FillInPlaceholders(string format, string accountName, string project, string repoId, int pullRequestId)
        {
            StringBuilder commandLineBuilder = new StringBuilder(format);
            commandLineBuilder.Replace("{accountName}", accountName);
            commandLineBuilder.Replace("{project}", project);
            commandLineBuilder.Replace("{repoId}", repoId);
            commandLineBuilder.Replace("{pullRequestId}", pullRequestId.ToString());

            string commandLine = commandLineBuilder.ToString();
            return commandLine;
        }
    }
}