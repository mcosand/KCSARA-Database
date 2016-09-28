namespace Sar.Database.Api.Extensions
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
