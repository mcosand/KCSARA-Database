using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kcsar.Membership
{
    public interface ISetPassword
    {
        void SetPassword(string username, string newPassword, bool sendMail);
    }
}
