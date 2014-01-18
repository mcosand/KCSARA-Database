/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Extensions
{
  using System;
  using System.Linq;
  using System.Reflection;

  public static class ExtensionList
  {
    public static Type[] GetExtensionInterfaces()
    {
      return Assembly.GetExecutingAssembly().ExportedTypes.Where(f => f.GetCustomAttribute<ExtensionInterfaceAttribute>() != null).ToArray();
    }
  }
}
