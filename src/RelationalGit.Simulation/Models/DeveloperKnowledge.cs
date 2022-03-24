using System.Collections.Generic;

namespace RelationalGit.Simulation
{
    public class DeveloperKnowledge
    {
        private HashSet<string> _committedFile = new HashSet<string>();

        private HashSet<string> _reviewedFile = new HashSet<string>();

        private HashSet<string> _touchedFiles = new HashSet<string>();

        private Dictionary<string, int> _reviewedFileCounts =   new Dictionary<string, int>();

        public int NumberOfTouchedFiles => _touchedFiles.Count;

        public int NumberOfReviewedFiles => _reviewedFile.Count;

        public int NumberOfCommittedFiles => _committedFile.Count;

        public int NumberOfCommits { get; set; }

        public int NumberOfReviews { get; set; }

        public int NumberOfContributions => NumberOfCommits + NumberOfReviews;

        public string DeveloperName { get; set; }

        public int NumberOfAuthoredLines { get; set; }

        /// <summary>
        /// Use this field when you want assign score to developers
        /// </summary>
        public double Score { get; set; }

        public bool IsFolderLevel { get; set; }

        public IEnumerable<string> GetTouchedFiles()
        {
            return _touchedFiles;
        }

        public Dictionary<string, int> GetReviewedFiles()
        {
            return _reviewedFileCounts;
        }
        public void AddCommittedFile(string fileName)
        {
            _committedFile.Add(fileName);
            _touchedFiles.Add(fileName);
        }

        public void AddReviewedFile(string fileName)
        {
            _reviewedFile.Add(fileName);
            if (!_reviewedFileCounts.ContainsKey(fileName))
            {
                _reviewedFileCounts[fileName] = 1;
            }else{
                _reviewedFileCounts[fileName] += 1;
            }
            _touchedFiles.Add(fileName);
        }
    }
}
