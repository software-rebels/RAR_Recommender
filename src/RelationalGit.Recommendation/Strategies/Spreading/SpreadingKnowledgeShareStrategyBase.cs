using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public abstract class SpreadingKnowledgeShareStrategyBase : RecommendationStrategy
    {
        public SpreadingKnowledgeShareStrategyBase(string knowledgeSaveReviewerReplacementType, ILogger logger,bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger,changePast)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            var availableDevs = AvailablePRKnowledgeables(pullRequestContext);

            if (availableDevs.Length == 0)
            {
                return Actual(pullRequestContext);
            }

            var simulationResults = new List<PullRequestKnowledgeDistribution>();
            var simulator = new PullRequestReviewSimulator(pullRequestContext, availableDevs, ComputeScore);

            foreach (var candidateSet in GetPossibleCandidateSets(pullRequestContext, availableDevs))
            {
                var simulationResult = simulator.Simulate(candidateSet.Reviewers, candidateSet.SelectedCandidateKnowledge);
                simulationResults.Add(simulationResult);
            }

            if (simulationResults.Count == 0)
            {
                return Actual(pullRequestContext);
            }

            var bestPullRequestKnowledgeDistribution = GetBestDistribution(simulationResults);

            return Recommendation(pullRequestContext, availableDevs, bestPullRequestKnowledgeDistribution);
        }

        private static PullRequestRecommendationResult Recommendation(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs, PullRequestKnowledgeDistribution bestPullRequestKnowledgeDistribution)
        {
            var recRevExp = 0.0;
            var val = 0.0;
            if (availableDevs.Length != 0)
            {
                val = pullRequestContext.ComputeBirdReviewerScore(availableDevs[0]);
                if (val != 0)
                {
                    recRevExp = val;
                }
            }
            var swappedRev = pullRequestContext.ActualReviewers.Except(bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers.ToArray()).ToArray();
            var swappedRevExp = 0.0;
            if (swappedRev.Length != 0)
            {
                val = pullRequestContext.ComputeBirdReviewerScore(swappedRev[0]);
                if (val != 0)
                {
                    swappedRevExp = val;
                }
            }
            return new PullRequestRecommendationResult(bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers.ToArray(), availableDevs, pullRequestContext.IsRisky(), pullRequestContext.Features, pullRequestContext.ComputeDefectPronenessScore(),pullRequestContext.ComputeMaxExpertise(bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers.ToArray()), pullRequestContext.PullRequest.Number,recRevExp, swappedRevExp);
        }

        private static PullRequestRecommendationResult Actual(PullRequestContext pullRequestContext)
        {
            return new PullRequestRecommendationResult(pullRequestContext.ActualReviewers, Array.Empty<DeveloperKnowledge>(), pullRequestContext.IsRisky(), pullRequestContext.Features, pullRequestContext.ComputeDefectPronenessScore(),pullRequestContext.ComputeMaxExpertise(pullRequestContext.ActualReviewers), pullRequestContext.PullRequest.Number,0,0);
        }

        internal PullRequestKnowledgeDistribution GetBestDistribution(List<PullRequestKnowledgeDistribution> simulationResults)
        {
            var maxScore = simulationResults.Max(q => q.PullRequestKnowledgeDistributionFactors.Score);
            return simulationResults.First(q => q.PullRequestKnowledgeDistributionFactors.Score == maxScore);
        }

        protected DeveloperKnowledge[] GetFolderLevelOweners(int depthToScanForReviewers, PullRequestContext pullRequestContext)
        {
            var pullRequestFiles = pullRequestContext.PullRequestFiles;
            var blameSnapshot = pullRequestContext.KnowledgeMap.BlameBasedKnowledgeMap.GetSnapshopOfPeriod(pullRequestContext.PullRequestPeriod.Id);

            var relatedFiles = new HashSet<string>();

            foreach (var pullRequestFile in pullRequestFiles)
            {
                var canonicalPath = pullRequestContext.CanononicalPathMapper[pullRequestFile.FileName];
                if (canonicalPath == null)
                {
                    continue;
                }

                var actualPath = blameSnapshot.GetActualPath(canonicalPath);

                if (actualPath == null)
                {
                    continue;
                }

                var neighbors = blameSnapshot.Trie.GetFileNeighbors(depthToScanForReviewers, actualPath);

                if (neighbors != null)
                {
                    foreach (var neighbor in neighbors)
                    {
                        relatedFiles.Add(neighbor);
                    }
                }
            }

            var developersKnowledge = new Dictionary<string, DeveloperKnowledge>();

            foreach (var relatedFile in relatedFiles)
            {
                TimeMachine.AddFileOwnership(pullRequestContext.KnowledgeMap, blameSnapshot, developersKnowledge, relatedFile, pullRequestContext.CanononicalPathMapper);
            }

            var folderLevelKnowlegeables = developersKnowledge.Values.Where(q => pullRequestContext.AvailableDevelopers.Any(d => d.NormalizedName == q.DeveloperName)).ToArray();

            foreach (var folderLevelKnowlegeable in folderLevelKnowlegeables)
            {
                folderLevelKnowlegeable.IsFolderLevel = true;
            }

            return folderLevelKnowlegeables;
        }

        private static double ComputeBirdReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var score = 0.0;

            foreach (var pullRequestFile in pullRequestContext.PullRequestFiles)
            {
                var canonicalPath = pullRequestContext.CanononicalPathMapper.GetValueOrDefault(pullRequestFile.FileName);
                if (canonicalPath == null)
                {
                    continue;
                }

                var fileExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetFileExpertise(canonicalPath);

                if (fileExpertise.TotalComments == 0)
                {
                    continue;
                }

                var reviewerExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetReviewerExpertise(canonicalPath, reviewer.DeveloperName);

                if (reviewerExpertise == (0, 0, null))
                {
                    continue;
                }

                var scoreTotalComments = reviewerExpertise.TotalComments / (double)fileExpertise.TotalComments;
                var scoreTotalWorkDays = reviewerExpertise.TotalWorkDays / (double)fileExpertise.TotalWorkDays;
                var scoreRecency = (fileExpertise.RecentWorkDay == reviewerExpertise.RecentWorkDay)
                    ? 1
                    : 1 / (fileExpertise.RecentWorkDay - reviewerExpertise.RecentWorkDay).Value.TotalDays;

               score += scoreTotalComments + scoreTotalWorkDays + scoreRecency;
            }

            return score / (3 * pullRequestContext.PullRequestFiles.Length);
        }
        internal abstract IEnumerable<(IEnumerable<DeveloperKnowledge> Reviewers, IEnumerable<DeveloperKnowledge> SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs);

        internal abstract DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext);

        internal abstract double ComputeScore(PullRequestContext pullRequestContext, PullRequestKnowledgeDistributionFactors pullRequestKnowledgeDistributionFactors);

        internal abstract double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer);
    }
}
