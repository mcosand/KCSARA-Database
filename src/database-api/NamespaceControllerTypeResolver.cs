using System;
using System.Web.Http.Dispatcher;

namespace Kcsara.Database.Api
{
  public class NamespaceControllerTypeResolver : DefaultHttpControllerTypeResolver
  {
    public NamespaceControllerTypeResolver(string ns) : base(t => IsHttpEndpoint(t, ns))
    {
    }

    internal static bool IsHttpEndpoint(Type t, string ns)
    {
      if (t == null) throw new ArgumentNullException("t");

      return t.IsClass &&
        t.IsVisible &&
        !t.IsAbstract &&
        !string.IsNullOrWhiteSpace(t.Namespace) &&
        t.Namespace.StartsWith(ns);
    }
  }
}
