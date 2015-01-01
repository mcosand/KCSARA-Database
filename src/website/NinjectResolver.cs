/*
 * Copyright 2013-2014 Matthew Cosand
 * Adapted from http://www.strathweb.com/2012/05/using-ninject-with-the-latest-asp-net-web-api-source/ by Filip W.
 */
namespace Kcsara.Database.Web
{
  using System.Web.Http.Dependencies;
  using Ninject;

  /// <summary>Dependency Resolver for WebAPI using Ninject.</summary>
  public class NinjectResolver : NinjectScope, IDependencyResolver
  {
    private IKernel kernel;

    /// <summary>Default constructor</summary>
    /// <param name="kernel">The composition root.</param>
    public NinjectResolver(IKernel kernel)
      : base(kernel)
    {
      this.kernel = kernel;
    }

    /// <summary>Mark the beginning of a dependency scope.</summary>
    /// <returns>A new dependency scope.</returns>
    public IDependencyScope BeginScope()
    {
      return new NinjectScope(this.kernel.BeginBlock());
    }
  }
}
