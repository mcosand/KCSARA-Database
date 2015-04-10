/*
 * Copyright 2012-2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System;
  using Kcsar.Database.Data;
  using Kcsara.Database.Services;
  using log4net;

  public abstract class BaseApiController : NoDataBaseApiController, IDisposable
  {
    public BaseApiController(IKcsarContext db, ILog log)
      : this(db, Ninject.ResolutionExtensions.Get<IAuthService>(MvcApplication.myKernel), log)
    {
    }

    public BaseApiController(IKcsarContext db, IAuthService auth, ILog log)
      : base(auth, log)
    {
      this.db = db;
    }

    protected readonly IKcsarContext db;

    protected override void Dispose(bool disposing)
    {
      IDisposable disposeDb = this.db as IDisposable;
      if (disposing && disposeDb != null) disposeDb.Dispose();
      base.Dispose(disposing);
    }
  }
}
