/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Membership
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Security;
    using Kcsar.Database.Model;

    /// <summary>
    /// 
    /// </summary>
    public class DatabaseUserProfile : KcsarUserProfile
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static KcsarUserProfile GetUserProfile(string username)
        {
            return Create(username) as KcsarUserProfile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static KcsarUserProfile GetUserProfile()
        {
            return Create(Membership.GetUser().UserName) as KcsarUserProfile;
        }

        /// <summary>
        /// 
        /// </summary>
        private bool searchedForMember = false;

        /// <summary>
        /// 
        /// </summary>
        private Member member = null;

        /// <summary>
        /// 
        /// </summary>
        private static Dictionary<string, Func<Member, object>> overrides = new Dictionary<string, Func<Member, object>>();

        /// <summary>
        /// 
        /// </summary>
        static DatabaseUserProfile()
        {
            overrides.Add("FirstName", x => x.FirstName);
            overrides.Add("LastName", x => x.LastName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public override object this[string propertyName]
        {
            get
            {
                LoadMember();

                if (propertyName != "LinkKey" && this.UsesLink && overrides.ContainsKey(propertyName) && member != null)
                {
                    return overrides[propertyName](member);
                }
                else
                {
                    return base[propertyName];
                }
            }
            set
            {
                if (propertyName != "LinkKey" && this.UsesLink && overrides.ContainsKey(propertyName))
                {
                    throw new InvalidOperationException(string.Format("Must change value '{0}' by using the database", propertyName));
                }

                base[propertyName] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool UsesLink
        {
            get
            {
                return !(string.IsNullOrEmpty(this.LinkKey) || this.LinkKey.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual UserSettings DatabaseSettings
        {
            get { return (UserSettings)this["DatabaseSettings"]; }
            set { this["DatabaseSettings"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool LoadMember()
        {
            if (member == null && searchedForMember == false)
            {
                searchedForMember = true;

                string key = this.LinkKey;
                if (string.IsNullOrEmpty(key))
                {
                    return false;
                }

                Guid id = new Guid(key);
                using (var context = new KcsarContext())
                {
                    member = (from m in context.Members where m.Id == id select m).FirstOrDefault();
                 //   this.db.Detach(member);
                }
                if (member == null)
                {
                    this.LinkKey = null;
                }
            }
            return member != null;
        }
    }
}
