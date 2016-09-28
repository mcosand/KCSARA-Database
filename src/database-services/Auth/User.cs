using System;
using System.ComponentModel.DataAnnotations;

namespace Sar.Database.Services
{
  public class User
  {
    public Guid Id { get; set; }
    public string Username { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    public string Name { get { return $"{FirstName} {LastName}"; } }

    public string Email { get; set; }   
  }
}
