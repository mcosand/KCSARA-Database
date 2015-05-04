/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq.Expressions;
  using Kcsar.Database.Data;
  using log4net;

  public abstract class BaseDataService
  {
    protected readonly Func<IKcsarContext> dbFactory;
    protected readonly ILog log;

    public BaseDataService(Func<IKcsarContext> dbFactory, ILog log)
    {
      this.log = log;
      this.dbFactory = dbFactory;
    }

    private Dictionary<string, object> funcCache = new Dictionary<string, object>();
    private object funcCacheLock = new object();

    protected T GetFunc<T>(Expression<T> expression)
    {
      object method;
      string key = expression.ToString();
      lock (funcCacheLock)
      {
        if (!funcCache.TryGetValue(key, out method))
        {
          method = expression.Compile();
          funcCache.Add(key, method);
        }
      }
      return (T)method;
    }
  }
}
