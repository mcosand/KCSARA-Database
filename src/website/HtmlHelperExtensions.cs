using Microsoft.AspNet.Mvc.Rendering;
using Newtonsoft.Json;

namespace Kcsara.Database.Web
{
  public static class HtmlHelperExtensions
  {
    public static HtmlString ToJson(this IHtmlHelper<dynamic> helper, object obj)
    {
      return new HtmlString(JsonConvert.SerializeObject(obj, Kcsar.Utils.GetJsonSettings()));
    }
  }
}
