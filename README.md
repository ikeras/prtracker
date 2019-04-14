# PRTracker

PRTracker runs in the task tray on Windows machines and keeps track of a configurable set of PR queries against Azure DevOps and GitHub accounts. PRTracker has logic to determine what set of PRs in the monitored accounts require your action. Further, it allows custom review tool behavior when a PR is double-clicked. 

## Configuration

PRTracker expects a config.json to be placed alongside the executable. This config file controls the accounts to monitor, as well as which review to invoke when a PR is double-clicked. The format of the config.json file appears below. The following replacement strings can be used within the commandLine or arguments property of reviewTools:

* {accountName}: replaced with the account name specified in the queries section, may be null
* {project}: replaced with the project specified in the queries section
* {owner}: replaced with the owner specified in the queries section
* {repoName}: replaced with the repoName specified in the queries section
* {pullRequestId}: replaced with the numeric identifier of the pull request that was double-clicked on

```
{
  "version": 0.2,
  "reviewTools": [
    {
      "name": "BrowserAzureDevOps",
      "commandLine": "https://dev.azure.com/{accountName}/{project}/_git/{repoName}/pullrequest/{pullRequestId}?_a=overview"
    },
    {
      "name": "BrowserGitHub",
      "commandLine": "https://github.com/{owner}/{repoName}/pull/{pullRequestId}"
    }
  ],
  "azureDevOps": {
	  "defaultReviewTool": "BrowserAzureDevOps",
	  "queries": [
		{
		  "accountName": "<Azure DevOps account to be accessed>",
		  "personalAccessToken": "<Azure DevOps PAT with access to this account, project and repo>",
		  "project": "<Azure DevOps Project>",
		  "repoName": "<Azure DevOps repo name>",
		  "reviewTool":  "<optional property that specifes the name of a tool specified in reviewTools, useful to override the defaultReviewTool>",
		  "uniqueUserId": "<Azure DevOps unique user ID, usually an email - this will cause the tool to omit any PRs that have been approved by this user>"
		}
	},
	"gitHub": {
		"defaultReviewTool": "BrowserGitHub",
		"queries": [
		  {
			"personalAccessToken": "<GitHub PAT with access to this repo>",
			"owner": "<GitHub owner/organization>",
			"repoName": "<GitHub repo name>",
			"uniqueUserId": "<GitHub user name>"
		  }
		]	
	}
    "updateInterval": <number of minutes to wait between updates>
  ]
}
```