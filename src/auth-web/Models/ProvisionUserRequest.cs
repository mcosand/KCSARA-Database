using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sar.Auth.Models
{
  public class ProvisionUserRequest : Member
  {
    public string Password { get; set; }
  }
}