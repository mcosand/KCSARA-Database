using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Kcsar.Database.Model;
using log4net;

namespace Kcsara.Database.Web.api
{
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