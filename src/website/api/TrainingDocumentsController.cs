/*
 * Copyright 2012-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.api
{
  using System;
  using Kcsar.Database.Data;
  using log4net;

  public class TrainingDocumentsController : DocumentsController
  {
    public TrainingDocumentsController(IKcsarContext db, ILog log)
      : base(db, log)
    { }

    protected override bool CanAddDocuments(Guid id)
    {
      return User.IsInRole("cdb.trainingeditors");
    }

    protected override string DocumentType
    {
      get { return "award"; }
    }
  }
}
