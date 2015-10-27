/*
 * Copyright 2009-2015 Matthew Cosand
 */

namespace Kcsar
{
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json.Serialization;

  public static class Utils
  {
    public static JsonSerializerSettings GetJsonSettings()
    {
      JsonSerializerSettings settings = new JsonSerializerSettings
      {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
      };
      settings.Converters.Add(new StringEnumConverter());
      return settings;
    }

    public const string IErrorLogConfigKey = "ErrorLoggerType";
  }
}
