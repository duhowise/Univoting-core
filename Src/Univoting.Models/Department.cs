using System;

namespace Univoting.Models
{
    public class Department:KeyModel
    {  
        public string Name { get; set; }
        public Guid ElectionId { get; set; }
    }
}