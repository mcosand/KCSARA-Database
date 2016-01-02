/*
 * Copyright 2009-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web
{
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json.Serialization;

  public static class Utils
  {
    public static JsonSerializerSettings GetJsonSettings()
    {
      return DecorateJsonSettings(new JsonSerializerSettings());
    }

    public static JsonSerializerSettings DecorateJsonSettings(JsonSerializerSettings settings)
    {
      settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
      settings.Converters.Add(new StringEnumConverter());
      return settings;
    }

    public const string IErrorLogConfigKey = "ErrorLoggerType";
  }
}
