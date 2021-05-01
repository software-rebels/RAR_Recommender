using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RelationalGit.Data;

namespace RelationalGit.Simulation
{
    public class PullRequestContext
    {
        private static Dictionary<long, (int TotalReviews, int TotalCommits)> _totalContributionsInPeriodDic = new Dictionary<long, (int TotalReviews, int TotalCommits)>();

        public string PRSubmitterNormalizedName { get; set; }

        public string SelectedReviewersType { get; set; }

        public string Features { get; set; }

        public Developer[] AvailableDevelopers;

        private bool? _isSafe;

        private static Dictionary<string, (int TotalReviews, int TotalCommits)> _contributionsDic = new Dictionary<string, (int TotalReviews, int TotalCommits)>();

        private static CommitComparer _commitComparer = new CommitComparer();

        private static PullRequestComparer _pullRequestComparer = new PullRequestComparer();
        private string[] _riskyFiles;

        public DeveloperKnowledge[] ActualReviewers { get;   set; }

        public PullRequestFile[] PullRequestFiles { get;   set; }

        public PullRequest PullRequest { get;   set; }

        public KnowledgeDistributionMap KnowledgeMap { get;   set; }

        public Dictionary<string, string> CanononicalPathMapper { get;   set; }

        public Period PullRequestPeriod { get;   set; }

        public ReadOnlyDictionary<string, Developer> Developers { get;   set; }

        public BlameSnapshot Blames { get;   set; }

        public DeveloperKnowledge[] PullRequestKnowledgeables { get;   set; }

        public Dictionary<long, Period> Periods { get;   set; }

        public HashSet<string> Hoarders { get;   set; } = null;

        public Dictionary<string,List<string>> _fileOwners { get; set; } = null;

        public Data.CommittedChange[] BugFixCommitChanges { get; set; } = null;

        public double? defectPronness { get; set; } = null;
        public int? NumberOfExperts { get; set; } = null;

        public bool PullRequestFilesAreSafe
        {
            get
            {

                if (_isSafe == null)
                {
                    FindHoarders();
                }

                return _isSafe.Value;
            }
        }

        public bool IsHoarder(string normalizedDeveloperName)
        {
            if (Hoarders == null)
            {
                FindHoarders();
            }

            return Hoarders.Contains(normalizedDeveloperName);
        }

        private void FindHoarders()
        {
            Hoarders = new HashSet<string>();
            _fileOwners = new Dictionary<string,List<string>>();

            var availableDevelopersOfPeriod = AvailableDevelopers.Select(q => q.NormalizedName).ToHashSet();
            var blameSnapshot = KnowledgeMap.CommitBasedKnowledgeMap;

            foreach (var pullRequestFile in PullRequestFiles)
            {
                var canonicalPath = CanononicalPathMapper.GetValueOrDefault(pullRequestFile.FileName);

                if (canonicalPath == null)
                {
                    continue;
                }

                var committers = KnowledgeMap.CommitBasedKnowledgeMap[canonicalPath]?.Where(q => q.Value.Developer.LastParticipationDateTime > PullRequest.CreatedAtDateTime)
                    ?.Select(q => q.Value.Developer.NormalizedName) ?? Array.Empty<string>();

                var reviewers = KnowledgeMap.ReviewBasedKnowledgeMap[canonicalPath]?.Where(q => q.Value.Developer.LastParticipationDateTime > PullRequest.CreatedAtDateTime)
                    ?.Select(q => q.Value.Developer.NormalizedName) ?? Array.Empty<string>();

                var availableContributors = committers.Union(reviewers).Where(q => availableDevelopersOfPeriod.Contains(q)).ToArray();

                if (availableContributors.Length < 2)
                {
                    _isSafe = false;

                    if (availableContributors.Length == 1)
                    {
                        Hoarders.Add(availableContributors[0]);
                    }
                }

                foreach (var availableContributor in availableContributors)
                {
                    if (!_fileOwners.ContainsKey(canonicalPath))
                    {
                        _fileOwners[canonicalPath] = new List<string>();
                    }

                    _fileOwners[canonicalPath].Add(availableContributor);
                }
            }

            if (!_isSafe.HasValue)
            {
                _isSafe = true;
            }
        }

        public string[] GetRiskyFiles(int riskOwnershipTreshold)
        {
            if (Hoarders == null)
            {
                FindHoarders();
            }

            if(_riskyFiles == null)
            {
                _riskyFiles = _fileOwners.Where(q => q.Value.Count() < riskOwnershipTreshold).Select(q => q.Key).ToArray();
            }

            return _riskyFiles;
        }

        public bool? IsRisky()
        {
            if (_riskyFiles == null)
                return null;
            
            return _riskyFiles.Length > 0;
        }

        public double GetEffort(string developer, int numberOfPeriodsForCalculatingProbabilityOfStay)
        {
            var lastYear = PullRequest.CreatedAtDateTime.Value.Subtract(TimeSpan.FromDays(365));

            var totalContribution = GetTotalContributionsBestweenPeriods(lastYear, PullRequest.CreatedAtDateTime.Value);
            var developerTotalContribution = GetDeveloperTotalContributionsBestweenPeriods(lastYear, PullRequest.CreatedAtDateTime.Value, developer);

            return ((developerTotalContribution.TotalReviews) + developerTotalContribution.TotalCommits)
                / (double)((totalContribution.TotalReviews) + totalContribution.TotalCommits);
        }
        
        private (int TotalReviews, int TotalCommits) GetDeveloperTotalContributionsBestweenPeriods(DateTime from, DateTime to, string developer)
        {
            var totalCommits = 0;
            var commits = KnowledgeMap.CommitBasedKnowledgeMap.GetDeveloperCommits(developer);
            var baselineCommit = new Commit() { AuthorDateTime = from };
            var index = commits.BinarySearch(baselineCommit,_commitComparer);

            if (index < 0)
                index = ~index;
            totalCommits += commits.Count - index;

            var totalReviews = 0;
            var reviews = KnowledgeMap.ReviewBasedKnowledgeMap.GetDeveloperReviews(developer);
            var baselinePullRequest = new PullRequest() { CreatedAtDateTime = from };

            index = reviews.BinarySearch(baselinePullRequest,_pullRequestComparer);

            if (index < 0)
                index = ~index;
            totalReviews += reviews.Count - index;

            return (totalReviews, totalCommits);
        }


        private (int TotalReviews, int TotalCommits) GetTotalContributionsBestweenPeriods(DateTime from, DateTime to)
        {
            var totalCommits = 0;
            var committers = KnowledgeMap.CommitBasedKnowledgeMap.GetCommitters();
            var baselineCommit = new Commit() { AuthorDateTime = from };

            foreach (var committer in committers)
            {
                var index = committer.Value.BinarySearch(baselineCommit,_commitComparer);

                if (index < 0)
                    index = ~index;
                totalCommits += committer.Value.Count - index;
            }

            var totalReviews = 0;
            var reviewers = KnowledgeMap.ReviewBasedKnowledgeMap.GetReviewers();
            var baselinePullRequest = new PullRequest() { CreatedAtDateTime = from };

            foreach (var reviewer in reviewers)
            {
                var index = reviewer.Value.BinarySearch(baselinePullRequest,_pullRequestComparer);

                if (index < 0)
                    index = ~index;
                totalReviews += reviewer.Value.Count - index;
            }       

            return (totalReviews, totalCommits);
        }

        public double GetProbabilityOfStay(string reviewer, int numberOfPeriodsForCalculatingProbabilityOfStay)
        {
            var currentPeriodId = PullRequestPeriod.Id;
            var lastYear = PullRequest.CreatedAtDateTime.Value.Subtract(TimeSpan.FromDays(365));

            var commitMonths = KnowledgeMap.CommitBasedKnowledgeMap.GetDeveloperCommits(reviewer)
                .Where(q=>q.AuthorDateTime>=lastYear)
                .Select(q=>q.AuthorDateTime.Month);
            
            var reviewMonths = KnowledgeMap.ReviewBasedKnowledgeMap.GetDeveloperReviews(reviewer)
                .Where(q => q.CreatedAtDateTime >= lastYear)
                .Select(q => q.CreatedAtDateTime.Value.Month);

            var numberOfContributedPeriodsSoFar = commitMonths.Union(reviewMonths).Count();

            return numberOfContributedPeriodsSoFar / (double) 12.0;
        }

        private (int TotalReviews, int TotalCommits) GetTotalContributionsOfPeriod(long periodId)
        {
            if (!_totalContributionsInPeriodDic.ContainsKey(periodId))
            {
                var totalCommits = Developers.Sum(q => q.Value.ContributionsPerPeriod.GetValueOrDefault(periodId)?.TotalCommits ?? 0);
                var totalReviews = Developers.Sum(q => q.Value.ContributionsPerPeriod.GetValueOrDefault(periodId)?.TotalReviews ?? 0);

                _totalContributionsInPeriodDic[periodId] = (totalReviews, totalCommits);
            }

            return _totalContributionsInPeriodDic[periodId];
        }
        
        public double ComputeMaxExpertise(DeveloperKnowledge[] selectedReviewers)
        {
            if(this.NumberOfExperts!=null){
                return (double)this.NumberOfExperts;
            }
            if(PullRequestFiles.Count()==0){
                return 0;
            }
            Dictionary<string, double> expertDic = new Dictionary<string, double>();
            foreach(var file in PullRequestFiles){
                var fileExpertise = KnowledgeMap.PullRequestEffortKnowledgeMap.GetFileExpertise(file.FileName);
                foreach(var reviewer in selectedReviewers){
                    // if(KnowledgeMap.PullRequestEffortKnowledgeMap.HasTouchedTheFile(file.FileName,reviewer.DeveloperName)){
                    //     expertDic[reviewer.DeveloperName]= 1;
                    // }
                    if(!expertDic.ContainsKey(reviewer.DeveloperName)){
                        expertDic[reviewer.DeveloperName] = 0;
                    }
                    if(!KnowledgeMap.PullRequestEffortKnowledgeMap.HasTouchedTheFile(file.FileName,reviewer.DeveloperName)){
                        continue;
                    }

                    var reviewerExpertise = KnowledgeMap.PullRequestEffortKnowledgeMap.GetReviewerExpertise(file.FileName, reviewer.DeveloperName);
                    double scoreTotalComments = 0;
                    scoreTotalComments = reviewerExpertise.TotalComments / (double)fileExpertise.TotalComments;

                    double scoreTotalWorkDays = 0;
                    scoreTotalWorkDays = reviewerExpertise.TotalWorkDays / (double)fileExpertise.TotalWorkDays;
                    var scoreRecency = (fileExpertise.RecentWorkDay == reviewerExpertise.RecentWorkDay)
                        ? 1
                        : 1 / (fileExpertise.RecentWorkDay - reviewerExpertise.RecentWorkDay).Value.TotalDays;

                    expertDic[reviewer.DeveloperName] += (scoreTotalComments + scoreTotalWorkDays + scoreRecency)/3;  
                }
            }
            foreach(var reviewer in selectedReviewers){
                expertDic[reviewer.DeveloperName] /= PullRequestFiles.Count();
            }
            return expertDic.Count()!=0 ? expertDic.Values.Max() : 0;
         }
        public double? ComputeDefectPronenessScore()
        {
            if(this.defectPronness!=null){
                return this.defectPronness;
            }

            if(PullRequestFiles.Count()==0){
                return 0;
            }
            double addLineCoefficient = 1;
            double delLineCoefficient = 1;
            double modificationLineCoefficient = 1;
            double preReleaseBugsCoefficient = 2;

            double additions = 0;
            double deletions = 0;
            double modifications = 0;
            double changes = 0;

            foreach (var file in PullRequestFiles)
            {
                additions += (double)file.Additions;
                deletions += (double)file.Deletions;
                modifications += (double)file.Changes;
            }
            var preReleaseBugCounts = new Dictionary<string, int>();

            foreach(var file in BugFixCommitChanges){
                if(!preReleaseBugCounts.ContainsKey(file.Path)){
                    preReleaseBugCounts[file.Path] = 1;
                    continue;
                }
                preReleaseBugCounts[file.Path] += 1;
            }
            var preReleaseBugScores =  new Dictionary<string, double>();
            foreach(var file in PullRequestFiles){
                if(!preReleaseBugCounts.ContainsKey(file.FileName)){
                    preReleaseBugScores[file.FileName] = 0;
                    continue;
                }
                preReleaseBugScores[file.FileName] = 1 -  Math.Pow(0.5,preReleaseBugCounts[file.FileName]);
            }

            var overallPreReleaseBugScore = preReleaseBugScores.Sum(q=>q.Value)/PullRequestFiles.Count();
            changes = additions + deletions + modifications;
            if(changes==0){
                this.defectPronness = 0;
            }else{
                this.defectPronness = (addLineCoefficient * (additions/changes) + delLineCoefficient * (deletions/changes) + 
                                        modificationLineCoefficient * (modifications/changes) + preReleaseBugsCoefficient * overallPreReleaseBugScore ) / (addLineCoefficient+ delLineCoefficient+modificationLineCoefficient+preReleaseBugsCoefficient);
            }

            return this.defectPronness;
        }
    }

    

    public class CommitComparer : IComparer<Commit>
    {
        public int Compare(Commit x, Commit y)
        {
            return x.AuthorDateTime.CompareTo(y.AuthorDateTime);
        }
    }

    public class PullRequestComparer : IComparer<PullRequest>
    {
        public int Compare(PullRequest x, PullRequest y)
        {
            return x.CreatedAtDateTime.Value.CompareTo(y.CreatedAtDateTime);
        }
    }
}
