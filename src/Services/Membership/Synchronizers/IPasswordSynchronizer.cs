/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System.Collections.Generic;

namespace Kcsara.Database.Web.Membership
{
  public interface IPasswordSynchronizer
  {
    string Name { get; }
    void SetPassword(string username, string newPassword);
  }

  public interface IPasswordSynchronizerWithOptions : IPasswordSynchronizer
  {
    void SetOptions(Dictionary<string, string> options);
  }
}
