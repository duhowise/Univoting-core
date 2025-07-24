using System;

namespace Univoting.Akka.SharedModels
{
    public class SkippedVote:KeyModel
    {
        public Voter Voter { get; set; }
        public Guid VoterId { get; set; }
        public DateTime Time { get; set; }
        public Position Position { get; set; }
        public Guid PositionId { get; set; }
    }
}