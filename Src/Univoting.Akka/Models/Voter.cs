using System;
using System.Collections.Generic;

namespace Univoting.Akka.Models
{
    public class Voter:KeyModel
    {
        public string Name { get; set; }
        public string IdentificationNumber { get; set; }
        public VotingStatus VotingStatus { get; set; }
        public ICollection<Vote> Votes { get; set; }
        public ICollection<SkippedVote> SkippedVotes { get; set; }

        public Guid ElectionId { get; set; }
    }
}