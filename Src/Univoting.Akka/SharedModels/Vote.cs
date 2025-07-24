using System;
using System.Collections.Generic;

namespace Univoting.Akka.SharedModels
{
    public class Vote:KeyModel
    {
        public Voter Voter { get; set; }
        public Guid VoterId { get; set; }
        public Candidate Candidate { get; set; }
        public Guid CandidateId { get; set; }
        public DateTime Time { get; set; }
        public Position Position { get; set; }
        public Guid PositionId { get; set; }

    }
}