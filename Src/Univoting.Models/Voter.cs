using System;
using System.Collections.Generic;

namespace Univoting.Models
{
    public class Voter:KeyModel
    {
        public string Name { get; set; }
        public string IdentificationNumber { get; set; }
        public VotingStatus VotingStatus { get; set; }
        public ICollection<Vote> Votes { get; set; }

        public Guid ElectionId { get; set; }
        public Election Election { get; set; }
    }
}