using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sar.Auth
{
  public enum ProcessVerificationResult
  {
    Success,
    EmailNotAvailable,
    AlreadyRegistered,
    InvalidVerifyCode
  }
}