using System;
using System.Collections.Generic;

namespace Univoting.Models
{
    public class Candidate:KeyModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] Picture { get; set; }
        public Position Position { get; set; }
        public Guid PositionId { get; set; }
        public ICollection<Vote> Votes { get; set; }
        public Priority Priority { get; set; }
        public Guid RankId { get; set; }
        public Guid ElectionId { get; set; }


    }
}