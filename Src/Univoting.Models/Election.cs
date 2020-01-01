using System.Collections.Generic;

namespace Univoting.Models
{
    public class Election:KeyModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] Logo { get; set; }
        public string BrandColour { get; set; }
        public ICollection<Voter> Voters { get; set; }
        public ICollection<Position> Positions { get; set; }
        public ICollection<Moderator> Moderators { get; set; }
    }
}
