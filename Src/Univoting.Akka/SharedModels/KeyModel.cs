using System;
using System.ComponentModel.DataAnnotations;

namespace Univoting.Akka.SharedModels
{
    public class KeyModel
    {
      [Key]  public Guid Id { get; set; }
      
    }
}