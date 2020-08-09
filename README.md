# PRTracker

PRTracker runs in the task tray on Windows machines and keeps track of a configurable set of PR queries against Azure DevOps and GitHub accounts. PRTracker has logic to determine what set of PRs in the monitored accounts require your action. Further, it allows custom review tool behavior when a PR is double-clicked. 

## Configuration

PRTracker expects a config.json to be placed alongside the executable. This config file controls the accounts to monitor, as well as which review to invoke when a PR is double-clicked. The format of the config.json file appears below. The following replacement strings can be used within the commandLine or arguments property of reviewTools:

* {accountName}: replaced with the account name specified in the queries section, may be null
* {project}: replaced with the project specified in the queries section
* {owner}: replaced with the owner specified in the queries section
* {repoName}: replaced with the repoName specified in the queries section
* {pullRequestId}: replaced with the numeric identifier of the pull request that was double-clicked on

###Remarks
* If isAssignedToMe is true, and filterToTeams isn't specified, all teams of which the user is a member are included in the query
* If filterToTeams is specified, then isAssignedToMe is assumed true
* includeDrafts default is false
```
{
  "version": 0.3,
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
            "filterToTeams": [<optional string array of teams of which the user is a member, to search for pull requests assigned to>"],
            "includeDrafts": <optional true | false>,
            "isAssignedToMe": <optional boolean that specifies to limit the query to pull requests assigend to the user and the teams they belong to>,
            "isCreatedByMe": <optional boolean that specifies to limit the query to pull requests created by teh uesr>,
            "personalAccessToken": "<Azure DevOps PAT with access to this account, project and repo>",
            "project": "<Azure DevOps Project>",
            "repoName": "<optional target Azure DevOps repo name>",
            "reviewTool":  "<optional property that specifes the name of a tool specified in reviewTools, useful to override the defaultReviewTool>",
            "sourceRefName": "<optional property that specifies source ref branch that the query should include for PRs>",
            "sourceRepoName": "<optional property that specifies the source repo of the PR to limit results to>",
            "status": "<optional property set to All, Closed, or Open with default being open>",
            "targetRefName": "<optional property to query for pull requests into this branch>",
            "uniqueUserId": "<optional Azure DevOps unique user ID, usually an email - this will cause the tool to omit any PRs that have been approved by this user>"
        }
    ],
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
}
