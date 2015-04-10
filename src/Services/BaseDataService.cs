/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
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
  }
}
