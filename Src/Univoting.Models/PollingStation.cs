using System;

namespace Univoting.Models
{
    public class PollingStation:KeyModel
    {
        public string Name { get; set; }
        public Guid ElectionId { get; set; }
        public Election Election { get; set; }
    }
}