/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models.Account
{
  using System.ComponentModel.DataAnnotations;

  public class LoginRequest
  {
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
  }
}