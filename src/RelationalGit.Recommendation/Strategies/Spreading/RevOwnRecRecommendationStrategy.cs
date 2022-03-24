using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class RevOwnRecRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        public RevOwnRecRecommendationStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests,string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests,recommenderOption, changePast)
        {
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var prFiles = pullRequestContext.PullRequestFiles.Select(q => pullRequestContext.CanononicalPathMapper[q.FileName]).Where(q => q != null).ToArray();
            string[] reviewedFiles = prFiles.Intersect(reviewer.GetReviewedFiles().Keys.ToArray()).ToArray();
            int ReviewsOfAllFiles = 0;
            foreach(var i in reviewedFiles)
            {
                if(reviewer.GetReviewedFiles()[i] > 1)
                {
                    ReviewsOfAllFiles += reviewer.GetReviewedFiles()[i];
                }
                else
                {
                    ReviewsOfAllFiles += 1;
                }
            }
             int totalReviews = pullRequestContext.PullRequestKnowledgeables.Sum(q => q.NumberOfReviews);

            if(totalReviews == 0)
            {
                return 0;
            }
            // var prevVal = reviewer.NumberOfReviews / (double)totalReviews;
            // double currentVal = ReviewsOfAllFiles / (double)totalReviews;
            // if (reviewer.NumberOfReviews != ReviewsOfAllFiles)
            // {
            //     var k = currentVal;
            //    return k;
            // }
            return ReviewsOfAllFiles / (double)totalReviews;
            // return reviewer.NumberOfReviews / (double)totalReviews;
        }
    }
}
