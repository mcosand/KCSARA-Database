namespace Sar.Database.Web.Auth
{
  public enum ProcessVerificationResult
  {
    Success,
    EmailNotAvailable,
    AlreadyRegistered,
    InvalidVerifyCode
  }
}