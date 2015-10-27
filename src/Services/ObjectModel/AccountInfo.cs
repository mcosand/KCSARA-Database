/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.ObjectModel.Accounts
{
  using System;

  public class AccountInfo
  {
    public string Name { get; set; }
    public bool Approved { get; set; }
    public bool Locked { get; set; }
    public DateTimeOffset LastActive { get; set; }
    public DateTimeOffset LastPassword { get; set; }
    public DateTimeOffset? LastLocked { get; set; }
    public string Email { get; set; }

    public string Password { get; set; }

    public Guid? MemberId { get; set; }
  }
}