using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsara.Database.Services.Accounts
{
  public enum RegistrationEmailStatus
  {
    NotFound,
    Invalid,
    Ready,
    Multiple,
    Registered
  }
}
