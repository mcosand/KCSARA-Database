using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using Kcsar.Database.Model;

    public class ExpirationView
    {
        public DateTime? Completed { get; set; }
        public DateTime? Date { get; set; }
        public ExpirationStatus Status { get; set; }

        public bool IsGood
        {
            get
            {
                return !(this.Status == ExpirationStatus.Expired
                     || this.Status == ExpirationStatus.RecentlyExpired
                     || this.Status == ExpirationStatus.NoEntry);
            }
        }

        public string StringValue(ExpirationStringType displayType)
        {
            if (this.Status == ExpirationStatus.NoEntry)
            {
                return "Missing";
            }
            else if ((this.Status == ExpirationStatus.Completed || this.Status == ExpirationStatus.CompletedNotRequired) && displayType != ExpirationStringType.NoExpiryAsBlank)
            {
                return ((displayType == ExpirationStringType.NoExpiryAsDateExpires) && this.Date.HasValue) ? string.Format("{0:yyyy-MM-dd}", this.Date) : "Complete";
            }

            return string.Format("{0:yyyy-MM-dd}", this.Date);
        }
    }

    public enum ExpirationStatus
    {
        Current,
        RecentlyExpired,
        NearExpiration,
        Expired,
        Completed,
        CompletedNotRequired,
        NotRequired,
        NoEntry
    }

    public enum ExpirationStringType
    {        
        NoExpiryAsDateExpires,
        NoExpiryAsComplete,
        NoExpiryAsBlank
    }
}
