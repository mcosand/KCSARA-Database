using Kcsara.Database.Web.Membership;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Kcsara.Database.Website.Tests
{
    public class LeakPasswordSynchronizer : IPasswordSynchronizer
    {
        string passwordFile;

        public LeakPasswordSynchronizer(string option, ILog log)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                throw new ArgumentException("Path to file required.", "option");
            }
            this.passwordFile = option;
        }

        public void SetPassword(string username, string newPassword)
        {
            File.AppendAllText(this.passwordFile, string.Format("{0} {1}: {2}{3}", DateTime.Now, username, newPassword, Environment.NewLine));
        }
    }
}
