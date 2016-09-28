namespace Sar.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using Api.Extensions;
  using Kcsar.Database.Model;
  using log4net;
  using Ninject;

  public class ExtensionProvider : IExtensionProvider
  {
    private readonly IKcsarContext db;
    private readonly ILog log;
    private readonly IKernel kernel;

    Dictionary<object, Dictionary<Type, Type>> extensions = new Dictionary<object, Dictionary<Type, Type>>();

    public ExtensionProvider(IKernel kernel, IKcsarContext db, ILog log)
    {
      this.db = db;
      this.log = log;
      this.kernel = kernel;
    }

    public void Initialize()
    {
      var extensionTypes = ExtensionList.GetExtensionInterfaces();

      IDbSet<SarUnit> units = this.db.Units;
      int unitCount = 0;
      try
      {
        unitCount = units.Local.Count;
      }
      catch (InvalidOperationException)
      {
        this.log.Error("Can't initialize database, does it need to be set up?");
      }

      foreach (var unit in units)
      {
        string assemblyName = string.Format("{0}.Extensions", unit.DisplayName);
        if (!File.Exists(Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), assemblyName + ".dll")))
        {
          log.InfoFormat("Unit {0} has no extension DLL", unit.DisplayName);
          continue;
        }
        var assembly = Assembly.Load(assemblyName);
        extensions.Add(unit.Id, new Dictionary<Type, Type>());

        foreach (var type in extensionTypes)
        {
          Type implementation = assembly.ExportedTypes.SingleOrDefault(f => type.IsAssignableFrom(f));
          if (implementation == null) continue;
          extensions[unit.Id].Add(type, implementation);
          log.InfoFormat("{0} added extension {1} for interface {2}", unit.DisplayName, implementation.Name, type.Name);
        }
      }
    }

    public T For<T>(SarUnit unit)
    {
      Dictionary<Type, Type> unitExtentions;
      if (this.extensions.TryGetValue(unit.Id, out unitExtentions))
      {
        Type implementationType;
        if (unitExtentions.TryGetValue(typeof(T), out implementationType))
        {
          return (T)this.kernel.Get(implementationType, new Ninject.Parameters.ConstructorArgument("unit", unit));
        }
      }
      return default(T);
    }
  }
}