using System;
using System.ComponentModel.DataAnnotations;

namespace Univoting.Models
{
    public class KeyModel
    {
      [Key]  public Guid Id { get; set; }
      
    }
}