namespace PRTracker.Models
{
    public class TrackerGitHubQuery
    {
        /// <summary>
        /// Gets or sets a filter for pull requests that are assigned to a certain user.
        /// </summary>
        public string AssginedTo { get; set; }

        /// <summary>
        /// Gets or sets a filter for pull requests based on the branch they are mergining into.
        /// </summary>
        public string Base { get; set; }

        /// <summary>
        /// Gets or sets a filter for pull requests that are created by a certain user.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets a filter for pull requests based on the branch they came from.
        /// </summary>
        public string Head { get; set; }

        /// <summary>
        /// Gets or sets a filter that finds pull requests that were created by a certain user, assigned to that user,
        /// mention that user, or were comment on by that user.
        /// </summary>
        public string InvolvedUser { get; set; }

        /// <summary>
        /// Gets or sets the GitHub PAT to use for authorization. Required.
        /// </summary>
        public string PersonalAccessToken { get; set; }

        /// <summary>
        /// Gets or sets the Review Tool to use when a PR returned by this query is launched.
        /// </summary>
        public string ReviewTool { get; set; }

        /// <summary>
        /// Gets or sets whether to query for pull requests that are in this state. If null, defaults to Active.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a user id used to filter the PR list if they have already reviewed that PR.
        /// </summary>
        public string UniqueUserId { get; set; }
    }
}
