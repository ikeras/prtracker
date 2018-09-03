using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace PRTrackerUI.Models
{
    internal class TrackerReviewTool
    {
        public string Name { get; set; }

        public string CommandLine { get; set; }

        public void Launch(string accountName, string project, string repoId, string pullRequestId)
        {
            StringBuilder commandLineBuilder = new StringBuilder(this.CommandLine);
            commandLineBuilder.Replace("{accountName}", accountName);
            commandLineBuilder.Replace("{project}", project);
            commandLineBuilder.Replace("{repoId}", repoId);
            commandLineBuilder.Replace("{pullRequestId}", pullRequestId);

            string commandLine = commandLineBuilder.ToString();

            Process.Start(commandLine);
        }
    }
}