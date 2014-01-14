using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Kcsara.Database.Web.api;
using M = Kcsar.Database.Model;
using Kcsara.Database.Web.api.Models;
using Kcsara.Database.Services;
using log4net;
using Twilio.TwiML;
using System.Xml.Linq;

namespace Kcsara.Database.Web.Areas.Missions.api
{
  public class TwilioApiController : BaseApiController
  {
    public TwilioApiController(M.IKcsarContext db, IAuthService auth, ILog log)
      : base(db, auth, log)
    { }

    [HttpGet]
    public XElement Start()
    {
      var result = new TwilioResponse();
      result.Say("This is a test");

      return result.Element;
    }
  }
}