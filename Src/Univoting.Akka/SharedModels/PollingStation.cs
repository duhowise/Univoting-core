using System;

namespace Univoting.Akka.SharedModels
{
    public class PollingStation:KeyModel
    {
        public string Name { get; set; }
        public Guid ElectionId { get; set; }
    }
}