/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web
{
  using Microsoft.AspNet.Mvc.Rendering;
  using Newtonsoft.Json;

  public static class HtmlHelperExtensions
  {
    public static HtmlString ToJson(this IHtmlHelper<dynamic> helper, object obj)
    {
      return new HtmlString(JsonConvert.SerializeObject(obj, Utils.GetJsonSettings()));
    }
  }
}
