
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;

    public static class Utils
    {
        public static readonly string[] CountyNames = new string[] { 
                    "Adams","Asotin","Benton","Chelan","Clallam","Clark","Columbia","Cowlitz","Douglas","Ferry","Franklin","Garfield","Grant","Grays Harbor","Island","Jefferson",
                    "King","Kitsap","Kittitas","Klickitat","Lewis","Lincoln","Mason","Okanogan","Pacific","Pend Oreille","Pierce","San Juan","Skagit","Skamania","Snohomish",
                    "Spokane","Stevens","Thurston","Wahkiakum","Walla Walla","Whatcom","Whitman","Yakima" };

        public class CaseInsensitiveComparer : IEqualityComparer<string>
        {
            #region IEqualityComparer<string> Members

            public bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.CurrentCultureIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                // TODO: I'm not at all sure this is the way this is supposed to be done.
                return obj.GetHashCode();
            }

            #endregion
        }

        #region Extensions
        public static Guid? ToGuid(this string src)
        {
            Guid? result = null;
            try
            {
                result = new Guid(src);
            }
            catch
            {
            }
            return result;
        }

        #endregion
    }
}
