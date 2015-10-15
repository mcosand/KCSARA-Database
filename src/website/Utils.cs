/*
 * Copyright 2009-2015 Matthew Cosand
 */

namespace Kcsar
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Net.Mail;
  using System.IO;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Serialization;
  using Newtonsoft.Json.Converters;

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

  public static bool CheckSimpleName(string name, bool throwOnError)
  {
    bool simple = true;

    if (string.IsNullOrEmpty(name))
    {
      simple = false;
      if (throwOnError)
      {
        throw new ArgumentException("Cannot be empty", name);
      }
    }

    if (!Regex.IsMatch(name, @"^[\._a-z0-9\-]+$", RegexOptions.IgnoreCase))
    {
      simple = false;
      if (throwOnError)
      {
        throw new ArgumentException("Can only contain numbers, letters, '.', '-', and '_'", name);
      }
    }

    return simple;
  }

  public static T CreateConfigurableInstance<T>(string configName, string defaultTypeName) where T : class
  {
    string typeName = ConfigurationManager.AppSettings[configName] ?? defaultTypeName;
    T manager;

    if (string.IsNullOrEmpty(typeName))
    {
      return null;
    }

    Uri remoteTypeLocation;
    if (Uri.TryCreate(typeName, UriKind.Absolute, out remoteTypeLocation))
    {
      // Create remote type
      // URI should be in form tcp://localhost:<port>/<path>
      manager = (T)Activator.GetObject(typeof(T), remoteTypeLocation.OriginalString);
    }
    else
    {
      Type t = Type.GetType(typeName);
      if (t == null)
      {
        throw new InvalidOperationException(string.Format("Can't load type '{0}' for config option {1}.", typeName, configName));
      }
      manager = (T)Activator.CreateInstance(Type.GetType(typeName));
    }

    return manager;
  }

  public static void SendMail(MailMessage msg)
  {
    // Send mail with the default timeout (per MSDN)
    SendMail(msg, 0);
  }

  public static void SendMail(MailMessage msg, int timeoutMS)
  {

    if (msg.From == null)
    {
      msg.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"] ?? "webpage@kcsar.local");
    }

    string mailFile = ConfigurationManager.AppSettings["Test_MailFile"];
    if (string.IsNullOrEmpty(mailFile))
    {
      SmtpClient client = new SmtpClient();
      client.Send(msg);
    }
    else
    {
      using (StreamWriter writer = new StreamWriter(mailFile, true))
      {
        writer.WriteLine("Mail to: " + msg.To.ToString());
        writer.WriteLine("Subject: " + msg.Subject);
        writer.WriteLine(msg.Body.Replace("\n", Environment.NewLine));
        writer.WriteLine("======================================\n");
      }
    }
  }

  #region Extensions
  public static Guid? ToGuid(this string src)
  {
    Guid? result = null;
    try
    {
      result = new Guid(src);
    }
    catch
    {
    }
    return result;
  }

  public static string Join(this IEnumerable<string> strings, string separator)
  {
    return string.Join(separator, strings.ToArray());
  }

  #endregion
}
}
