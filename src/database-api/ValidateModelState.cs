using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kcsara.Database.Api
{
  class ValidateModelState : ActionFilterAttribute
  {
    public override void OnActionExecuting(HttpActionContext actionContext)
    {
      var jsonSettings = actionContext.RequestContext.Configuration.Formatters.JsonFormatter.SerializerSettings;

      var resolver = (DefaultContractResolver)jsonSettings.ContractResolver;
      

      if (!actionContext.ModelState.IsValid)
      {
        var jsonBody = JsonConvert.SerializeObject(
          new
          {
            Errors = actionContext.ModelState
                        .Where(f => f.Value.Errors.Any())
                        .ToDictionary(
                            f => string.Join(".", f.Key.Split('.').Skip(1).Select(g => resolver.GetResolvedPropertyName(g))),
                            f => f.Value.Errors.Select(g => g.ErrorMessage).Distinct()
                        )
          },
          jsonSettings);

        actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, jsonBody);
      }
    }
  }
}
