using System.Collections.Generic;

namespace Univoting.Models
{
    public class Position:KeyModel
    {
        public string Name { get; set; }
        public ICollection<Vote> Votes { get; set; }
        public ICollection<SkippedVote> SkippedVotes { get; set; }
        public ICollection<Candidate> Candidates { get; set; }

    }
}