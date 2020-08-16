# PRTracker

PRTracker runs in the task tray on Windows machines and keeps track of a configurable set of PR queries against Azure DevOps and GitHub accounts. PRTracker has logic to determine what set of PRs in the monitored accounts require your action. Further, it allows custom review tool behavior when a PR is double-clicked. 

## Configuration

PRTracker expects a config.json to be placed alongside the executable. This config file controls the accounts to monitor, as well as which review to invoke when a PR is double-clicked. The format of the config.json file appears below. The following replacement strings can be used within the commandLine or arguments property of reviewTools:

* {accountName}: replaced with the account name specified in the queries section, may be null
* {project}: replaced with the project specified in the queries section
* {owner}: replaced with the owner specified in the queries section
* {repoName}: replaced with the repoName specified in the queries section
* {pullRequestId}: replaced with the numeric identifier of the pull request that was double-clicked on

The structure of the config file is shown in the code block below.

|Property|Required|Description|
|--------|--------|-----------|
|version|Yes|The version of the config file, currently 0.3|
|reviewTools|Yes|Contains an array of tools that can be launched by double-clicking on a PR from a query. Examples include a browser, VS Code, VS, etc.|
|reviewTools.name|Yes|A string property that names the tool, which can be referred to later by properties such as 'defaultReviewTool'|
|reviewTools.commandLine|Yes|The command line to execute when the tool is invoked. This can contain the replacements defined above (e.g. {accountName})|
|reviewTools.arguments|No|The arguments to pass to the command line
|azureDevOps|No|Holds any Azure DevOps queries|
|azureDevOps.defaultReviewTool|No|Property that refers to the name of any tool defined in the reviewTools section|
|azureDevOps.queries|No|An array of queries to execute against the Azure DevOps endpoint|
|azureDevOps.queries.accountName|Yes|The Azure DevOps account to query against|
|azureDevOps.queries.filterToTeams|No|An array of teams that the user belongs to, which should be queried for PRs|
|azureDevOps.queries.includeDrafts|No|A boolean that specifies whether to include draft PRs in the results|
|azureDevOps.queries.isAssignedToMe|No|A boolean that specifies whether to query for all PRs assigned to the person represented by the PAT|
|azureDevOps.queries.isCreatedByMe|No|A boolean that specifies whether to query for all PRs created by the person represented by the PAT|
|azureDevOps.queries.personalAccessToken|Yes|A PAT with access to this account and project|
|azureDevOps.queries.project|Yes|The Azure DevOps project to query against|
|azureDevOps.queries.repoName|No|Used to filter the results down to only those that target this repo|
|azureDevOps.queries.reviewTool|No|Specifes the name of a tool specified in reviewTools, useful to override the defaultReviewTool|
|azureDevOps.queries.sourceRefName|No|Filter to PRs that orinate from this branch|
|azureDevOps.queries.sourceRepoName|No|Filter to PRs that originate from this repo|
|azureDevOps.queries.status|No|A string specifying All, Closed, or Open indicating which PR states to include. Default is Open|
|azureDevOps.queries.targetRefName|No|Filter to PRs that target this branch|
|azureDevOps.queries.uniqueUserIdFilter|No|Filter query and omit PRs that have been approved by this user|
|gitHub|No|Holds any GitHub queries|
|gitHub.defaultReviewTool|No|Property that refers to the name of any tool defined in the reviewTools section|
|gitHub.queries|No|An array of queries to execute against the GitHub endpoint|
|gitHub.queries.assignedTo|No|Filter to PRs that are assigned to this GitHub identity|
|gitHub.queries.base|No|Filter to PRs that are merging into this branch|
|gitHub.queries.createdBy|No|Filter to PRs created by this GitHub identity|
|gitHub.queries.head|No|Filter to PRs that originate from this branch|
|gitHub.queries.invovledUser|No|Return PRs created, assigned to, mention, or were commented on by this user|
|gitHub.queries.personalAccessToken|Yes|A PAT with access to this account and project|
|gitHub.queries.reviewTool|No|Specifes the name of a tool specified in reviewTools, useful to override the defaultReviewTool|
|gitHub.queries.status|No|A string specifying All, Closed, or Open indicating which PR states to include. Default is Open|
|gitHub.queries.uniqueUserIdFilter|No|Filter query and omit PRs that have been approved by this user|
|updateInterval|Yes|Number of minutes to wait between updates|

```json
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
            "accountName": string,
            "filterToTeams": [string, string, etc.],
            "includeDrafts": true | false,
            "isAssignedToMe": string,
            "isCreatedByMe": string,
            "personalAccessToken": string,
            "project": string,
            "repoName": string,
            "reviewTool":  string,
            "sourceRefName": string,
            "sourceRepoName": string,
            "status": "All" | "Closed" | "Open",
            "targetRefName": string,
            "uniqueUserId": string
        }
    ],
  },
  "gitHub": {
    "defaultReviewTool": "BrowserGitHub",
    "queries": [
        {
            "assignedTo": string,
            "base": string,
            "createdBy": string.
            "head": string,
            "involvedUser": string,
            "personalAccessToken": string,
            "reviewTool": string,
            "status": "All" | "Closed" | "Open",
            "uniqueUserId": string
        }
    ]
  }
  "updateInterval": number
}
