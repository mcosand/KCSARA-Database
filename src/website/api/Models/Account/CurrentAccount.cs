/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models.Account
{
  public class CurrentAccount
  {
    public string Username { get; set; }
    public string FirstName { get; set; }
    public bool IsAuthenticated { get; set; }
  }
}