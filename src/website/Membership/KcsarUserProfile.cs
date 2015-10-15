/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Membership
{
  using System.Web.Profile;

  /// <summary>
  /// 
  /// </summary>
  /// <example>
  /// <code>
  /// &lt;profile inherits="Kcsar.Database.DatabaseUserProfile" defaultProvider="DatabaseProfileProvider"&gt;
  ///   &lt;providers&gt;
  ///     &lt;clear /&gt;
  ///     &lt;add name="DatabaseProfileProvider" type="Kcsar.Database.DatabaseProfileProvider" connectionStringName="AuthStore"/&gt;
  ///   &lt;/providers&gt;
  /// &lt;/profile>
  /// </code>
  /// </example>
  public class KcsarUserProfile : ProfileBase
  {
    /// <summary>
    /// 
    /// </summary>
    [SettingsAllowAnonymous(false)]
    public virtual string FirstName
    {
      get { return (string)this["FirstName"]; }
      set { this["FirstName"] = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    [SettingsAllowAnonymous(false)]
    public virtual string LastName
    {
      get { return (string)this["LastName"]; }
      set { this["LastName"] = value; }
    }
  }
}
