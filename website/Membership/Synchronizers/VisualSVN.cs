namespace Kcsara.Database.Web.Membership.Synchronizers
{
    using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

    public class VisualSVN : IPasswordSynchronizer
    {
        private string filePath;
        readonly ILog logger;
        public VisualSVN(string option, ILog logger)
        {
            this.logger = logger;
            this.filePath = option;
        }
        
        private static object fileLock = new object();
        public void SetPassword(string username, string newPassword)
        {
            List<string> entries = new List<string> {
                string.Format("{0}:{{SHA}}{1}", username, Convert.ToBase64String(SHA1Managed.Create().ComputeHash(Encoding.ASCII.GetBytes(newPassword))))
            };

            lock (fileLock)
            {
                logger.Info("Set SVN password for user " + username);
                if (File.Exists(this.filePath))
                {
                    entries.AddRange(File.ReadAllLines(this.filePath).Where(f => !f.StartsWith(username+":", StringComparison.OrdinalIgnoreCase)));
                }
                File.WriteAllLines(this.filePath, entries);
            }
        }
    }
}