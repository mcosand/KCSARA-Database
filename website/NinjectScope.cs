namespace Kcsara.Database.Web
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web.Http.Dependencies;
  using Ninject.Activation;
  using Ninject.Parameters;
  using Ninject.Syntax;

  /// <summary>Defines a dependency scope based on Ninject.</summary>
  public class NinjectScope : IDependencyScope
  {
    /// <summary>Reference to the composition root (kernel).</summary>
    protected IResolutionRoot resolutionRoot;
    
    /// <summary>Default constructor.</summary>
    /// <param name="kernel">The composition root.</param>
    public NinjectScope(IResolutionRoot kernel)
    {
      this.resolutionRoot = kernel;
    }
    
    /// <summary>Gets a service based on the specified type.</summary>
    /// <param name="serviceType">The requested service type.</param>
    /// <returns>An object of the requested type.</returns>
    public object GetService(Type serviceType)
    {
      IRequest request = resolutionRoot.CreateRequest(serviceType, null, new Parameter[0], true, true);
      return resolutionRoot.Resolve(request).SingleOrDefault();
    }

    /// <summary>Gets a list of services that have been bound to the specified type.</summary>
    /// <param name="serviceType">The requested service type.</param>
    /// <returns>A list of service instances.</returns>
    public IEnumerable<object> GetServices(Type serviceType)
    {
      IRequest request = resolutionRoot.CreateRequest(serviceType, null, new Parameter[0], true, true);
      return resolutionRoot.Resolve(request).ToList();
    }

    /// <summary>Disposes the current scope.</summary>
    public virtual void Dispose()
    {
      IDisposable disposable = (IDisposable)resolutionRoot;
      if (disposable != null) disposable.Dispose();
      resolutionRoot = null;
    }
  }
}