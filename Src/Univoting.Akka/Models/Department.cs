using System;

namespace Univoting.Akka.Models
{
    public class Department:KeyModel
    {  
        public string Name { get; set; }
        public Guid ElectionId { get; set; }
    }
}