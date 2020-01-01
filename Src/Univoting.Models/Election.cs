using System.Collections.Generic;

namespace Univoting.Models
{
    public class Election:KeyModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] Logo { get; set; }
        public string Colour { get; set; }
        public ICollection<Voter> Voters { get; set; }
        public ICollection<Position> Positions { get; set; }
    }
}
