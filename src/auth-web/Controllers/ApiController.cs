/* 
 * Copyright Matthew Cosand
 */
namespace Sar.Auth
{
  using System;
  using System.Net.Http;
  using System.Threading.Tasks;
  using System.Web;
  using System.Web.Http;
  using IdentityServer3.Core.Extensions;
  using Sar.Auth.Models;
  using Sar.Auth.Services;
  using Serilog;

  public class ApiController : System.Web.Http.ApiController
  {
    private readonly ILogger _log;
    private readonly SarUserService _userService;

    public ApiController(SarUserService service, ILogger log)
    {
      _userService = service;
      _log = log;
    }

    [HttpPost]
    [Route("externalVerificationCode")]
    public async Task<object> ExternalVerificationCode(VerifyCodeRequest request)
    {
      var ctx = HttpContext.Current.GetOwinContext();
      var partial_login = await ctx.Environment.GetIdentityServerPartialLoginAsync();
      if (partial_login == null)
      {
        throw new UserErrorException(Strings.NotLoggedInExternalLogin);
      }

      var processResult = await _userService.SendExternalVerificationCode(partial_login, request.Email);

      return ServiceResultToObject(request, processResult, new { Success = true });
    }

    [HttpPost]
    [Route("verifyExternalCode")]
    public async Task<object> VerifyExternalCode(VerifyCodeRequest request)
    {
      var ctx = HttpContext.Current.GetOwinContext();
      var partial_login = await ctx.Environment.GetIdentityServerPartialLoginAsync();
      if (partial_login == null)
      {
        throw new UserErrorException(Strings.NotLoggedInExternalLogin);
      }

      var processResult = await _userService.VerifyExternalCode(partial_login, request.Email, request.Code);

      var resumeUrl = await ctx.Environment.GetPartialLoginResumeUrlAsync();
      return ServiceResultToObject(request, processResult, new { Success = true, Url = resumeUrl });
    }

    [HttpGet]
    [Route("user/{sub}/roles")]
    public async Task<string[]> UserRoles(Guid userId)
    {
      if (Request.Headers.Authorization.Scheme != "Bearer" || string.IsNullOrWhiteSpace(Request.Headers.Authorization.Parameter))
      {
        throw new InvalidOperationException();
      }

      return new[] { "admin", "user" };
    }

    private static object ServiceResultToObject(VerifyCodeRequest request, ProcessVerificationResult processResult, object success)
    {
      object result;
      switch (processResult)
      {
        case ProcessVerificationResult.Success:
          result = success;
          break;
        case ProcessVerificationResult.AlreadyRegistered:
          result = new { Success = false, Errors = new { _ = new[] { Strings.ExternalLoginAlreadyRegistered } } };
          break;
        case ProcessVerificationResult.EmailNotAvailable:
          result = new { Success = false, Errors = new { Email = new[] { string.Format(Strings.ExternalLoginNotAvailable, request.Email) } } };
          break;
        case ProcessVerificationResult.InvalidVerifyCode:
          result = new { Success = false, Errors = new { Code = new[] { Strings.BadVerification } } };
          break;
        default:
          throw new NotImplementedException(string.Format(LogStrings.UnknownProcessVerification, processResult));
      }

      return result;
    }
  }
}
