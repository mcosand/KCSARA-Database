/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Model
{
  using System.ComponentModel.DataAnnotations;

  public class LoginRequest
  {
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
    
    public string ReturnUrl { get; set; }
    
    public int? Id { get; set; }

    public int? P { get; set; }
  }
}