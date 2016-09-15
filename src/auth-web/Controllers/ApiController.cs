/* 
 * Copyright Matthew Cosand
 */
namespace Sar.Auth
{
  using System;
  using System.Linq;
  using System.Net.Http;
  using System.Threading.Tasks;
  using System.Web;
  using System.Web.Http;
  using System.Web.Http.Cors;
  using IdentityServer3.Core.Extensions;
  using IdentityServer3.Core.Validation;
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
    public Task<string[]> UserRoles(Guid userId)
    {
      if (Request.Headers.Authorization.Scheme != "Bearer" || string.IsNullOrWhiteSpace(Request.Headers.Authorization.Parameter))
      {
        throw new InvalidOperationException();
      }

      return Task.FromResult(new[] { "admin", "user" });
    }

    [HttpGet]
    [Route("checkusername/{username}")]
    [EnableCors(origins: "*", headers: "*", methods: "GET")]
    public async Task<string> CheckUsername(string username)
    {
      return (await _userService.GetUserAsync(username) == null) ? "Available" : "NotAvailable";
    }

    [HttpPost]
    [Route("provision")]
    public async Task<object> ProvisionUser(ProvisionUserRequest body)
    {
      await VerifyAccountsClaim();

      var newAccount = await _userService.CreateUserAsync(body);

      return newAccount;
    }

    [HttpPost]
    [Route("linkmember")]
    public async Task<object> LinkMember(LinkMemberRequest body)
    {
      await VerifyAccountsClaim();

      await _userService.LinkMember(body);

      return new { status = "OK" };
    }

    private async Task VerifyAccountsClaim()
    {
      if (Request.Headers.Authorization == null
        || Request.Headers.Authorization.Scheme != "Bearer"
        || string.IsNullOrWhiteSpace(Request.Headers.Authorization.Parameter))
      {
        throw new HttpException(401, "Unauthorized");
      }
      var signing = Request.GetOwinContext().Environment.ResolveDependency<TokenValidator>();
      var validation = await signing.ValidateAccessTokenAsync(Request.Headers.Authorization.Parameter);
      if (validation.IsError || !validation.Claims.Any(f => f.Type == "scope" && f.Value == "accounts"))
      {
        _log.Warning("Tried to provision user with claims\n " + string.Join("\n", validation.Claims.Select(f => f.Type + ": " + f.Value)));
        throw new HttpException(401, "Unauthorized");
      }
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
