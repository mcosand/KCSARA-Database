namespace Sar.Database.Web.Auth
{
  public class VerifyCodeRequest
  {
    public string Email { get; set; }
    public string Code { get; set; }
  }
}