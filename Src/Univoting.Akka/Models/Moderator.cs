using System;

namespace Univoting.Akka.Models
{
    public class Moderator:KeyModel
    {
        public string Name { get; set; }
        public Badge Badge { get; set; }
        public Guid ElectionId { get; set; }
    }
}