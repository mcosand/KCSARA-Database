/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Membership
{
  using Kcsara.Database.Web.Membership;
  using Kcsara.Database.Web.Membership.Config;
  using log4net;
  using System;
  using System.Configuration;
  using System.Net.Mail;
  using System.Web.Profile;
  using System.Web.Security;
  using System.Linq;
  using System.Collections.Generic;
  using Newtonsoft.Json;
  using Kcsara.Database.Web;

  public class MembershipProvider : SqlMembershipProvider, ISetPassword
  {
    public void SetPassword(string username, string newPassword, bool sendMail)
    {
      // There's enough stuff going on here that it would be hard to return false.
      // What would false mean? All passwords failed? At least one password failed?
      // Therefore, we throw on failure.

      List<Exception> exceptions = new List<Exception>();
      PasswordSyncSection syncSection = ConfigurationManager.GetSection("passwordSync") as PasswordSyncSection;
      if (syncSection != null)
      {
        foreach (SynchronizerElement config in syncSection.Items)
        {
          Type syncType = Type.GetType(config.Type);
          if (syncType != null)
          {
            string name = syncType.Name;
            try
            {
              IPasswordSynchronizer sync = Activator.CreateInstance(syncType, config.Option, LogManager.GetLogger(syncType.Name)) as IPasswordSynchronizer;
              name = sync.Name;

              IPasswordSynchronizerWithOptions optionsSync = sync as IPasswordSynchronizerWithOptions;
              if (optionsSync != null && config.Items.Count > 0)
              {
                optionsSync.SetOptions(config.Items.Cast<SynchronizerOptionElement>().ToDictionary(f => f.Key, f => f.Value));
              }
              sync.SetPassword(username, newPassword);
            }
            catch (Exception ex)
            {
              ex.Data.Add("name", name);
              exceptions.Add(ex);
            }
          }
        }
      }

      try
      {
        string tempPassword = base.ResetPassword(username, null);
        base.ChangePassword(username, tempPassword, newPassword);
      }
      catch (Exception ex)
      {
        ex.Data.Add("name", Strings.DatabaseName);
        exceptions.Add(ex);
      }

      if (exceptions.Count > 0)
      {
        throw new AggregateException(exceptions);
      }

      if (sendMail)
      {
        MembershipUser user = this.GetUser(username, false);

        MailMessage msg = new MailMessage();
        msg.To.Add(user.Email);
        msg.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"] ?? "webpage@kcsar.local", "KCSARA Web Page");
        msg.Subject = "KCSARA Password Changed";
        msg.Body = string.Format("You password has been changed.\n\nUsername: {0}\nPassword: {1}", username, newPassword);

        Utils.SendMail(msg);
      }
    }

    public static Guid? UsernameToMemberKey(string name)
    {
      KcsarUserProfile profile = ProfileBase.Create(name) as KcsarUserProfile;
      if (profile.UsesLink)
      {
        return new Guid(profile.LinkKey);
      }
      return null;
    }
  }
}
