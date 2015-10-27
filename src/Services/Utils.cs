/*
 * Copyright 2009-2015 Matthew Cosand
 */

namespace Kcsara.Database.Services
{
  using System;
  using System.Text.RegularExpressions;

  public static class Utils
  {
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
  }
}
