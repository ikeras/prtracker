# PRTracker

PRTracker runs in the task tray on Windows machines and keeps track of a configurable set of PR queries against VSTS accounts. PRTracker has logic to determine what set of PRs in the monitored accounts require your action. Further, it allows custom review tool behavior when a PR is double-clicked. 

## Configuration

PRTracker expects a config.json to be placed alongside the executable. This config file controls the accounts to monitor, as well as which review to invoke when a PR is double-clicked. The format of the config.json file appears below. The following replacement strings can be used within the commandLine or arguments property of reviewTools:

* {accountName}: replaced with the account name specified in the queries section
* {project}: replaced with the project specified in the queries section
* {repoId}: replaced with the repoId specified in the queries section
* {pullRequestId}: replaced with the numeric identifier of the pull request that was double-clicked on

```
{
  "version": 0.1,
  "reviewTools": [
    {
      "name": "Browser",
      "commandLine": "https://{accountName}.visualstudio.com/{project}/_git/{repoId}/pullrequest/{pullRequestId}?_a=overview"
    }
  ],
  "defaultReviewTool": "Browser",
  "queries": [
    {
      "accountName": "<VSTS account to be accessed>",
      "personalAccessToken": "<VSTS PAT with access to this account, project and repo>",
      "project": "<VSTS Project>",
      "repoId": "<VSTS repo name>",
      "reviewTool":  "<optional property that specifes the name of a tool specified in reviewTools, useful to override the defaultReviewTool>"
    }
  ]
}
```