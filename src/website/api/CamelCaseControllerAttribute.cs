using System;
using System.Linq;
using System.Web.Http.Controllers;
using System.Net.Http.Formatting;

public class CamelCaseControllerConfigAttribute : Attribute, IControllerConfiguration
{
  public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
  {
    var formatter = controllerSettings.Formatters.OfType<JsonMediaTypeFormatter>().Single();
    controllerSettings.Formatters.Remove(formatter);

    formatter = new JsonMediaTypeFormatter
    {
      SerializerSettings = Kcsar.Utils.GetJsonSettings()
    };

    controllerSettings.Formatters.Add(formatter);
  }
}