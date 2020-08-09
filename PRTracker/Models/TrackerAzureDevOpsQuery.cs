using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace PRTracker.Models
{
    public class TrackerAzureDevOpsQuery
    {
        /// <summary>
        /// Gets or sets the Azure DevOps account to query against. Required.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets whether to query for a subset of the teams which the uesr is a member of. IsAssignedToMe must be true to set this property.
        /// </summary>
        public string[] FilterToTeams { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include draft PRs in the results.
        /// </summary>
        public bool IncludeDrafts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to limit query for pull requests reviewed by me and teams of which I'm a member.
        /// </summary>
        public bool IsAssignedToMe { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to limit query for pull requests created by me.
        /// </summary>
        public bool IsCreatedByMe { get; set; }

        /// <summary>
        /// Gets or sets the Azure DevOps PAT to use for authorization. Required.
        /// </summary>
        public string PersonalAccessToken { get; set; }

        /// <summary>
        /// Gets or sets the Azure DevOps Project to query against. Required.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets whether to query for pull requests whose target branch is in this repository.
        /// </summary>
        public string RepoName { get; set; }

        /// <summary>
        /// Gets or sets the Review Tool to use when a PR returned by this query is launched.
        /// </summary>
        public string ReviewTool { get; set; }

        /// <summary>
        /// Gets or sets whether to query for pull requests from this branch.
        /// </summary>
        public string SourceRefName { get; set; }

        /// <summary>
        /// Gets or sets whether to query for pull requests whose source branch is in this repository.
        /// </summary>
        public string SourceRepoName { get; set; }

        /// <summary>
        /// Gets or sets whether to query for pull requests that are in this state. If null, defaults to Active.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets whether to query for pull requests into this branch.
        /// </summary>
        public string TargetRefName { get; set; }

        /// <summary>
        /// Gets or sets a user id used to filter the PR list if they have already reviewed that PR.
        /// </summary>
        public string UniqueUserId { get; set; }
    }
}
