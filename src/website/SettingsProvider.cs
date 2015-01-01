/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Kcsar.Database;
    using System.Threading;
    using System.Web.Security;
    using System.Web.Profile;
    using Kcsar.Membership;

    public static class SettingsProvider
    {
        public static UserSettings LoadSettings(Func<string, object> GetSessionValue)
        {
            if (string.IsNullOrEmpty(Thread.CurrentPrincipal.Identity.Name))
            {
                return new UserSettings();
            }

            UserSettings settings = null;
            if (GetSessionValue("settings") != null)
            {
                settings = (UserSettings)GetSessionValue("settings");
            }
            else
            {
                MembershipUser account = System.Web.Security.Membership.GetUser(Thread.CurrentPrincipal.Identity.Name);
                DatabaseUserProfile profile = ProfileBase.Create(account.UserName) as DatabaseUserProfile;
                if (profile.DatabaseSettings != null)
                {
                    settings = profile.DatabaseSettings as UserSettings;
                }
            }
            return settings ?? new UserSettings();
        }

        public static void SaveSettings(UserSettings settings, Action<string, object> SetSessionValue, bool persist)
        {
            SetSessionValue("settings", settings);

            MembershipUser account = System.Web.Security.Membership.GetUser(Thread.CurrentPrincipal.Identity.Name);
            DatabaseUserProfile profile = ProfileBase.Create(account.UserName) as DatabaseUserProfile;
            profile.DatabaseSettings = settings;
            profile.Save();
        }
    }
}
