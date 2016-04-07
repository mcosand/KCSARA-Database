/*
 * Copyright 2013-2014 Matthew Cosand
 */
using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Kcsar.Database.Model;
using Kcsara.Database.Web.api;
using Moq;
using NUnit.Framework;

namespace Internal.Website.Api
{
  [TestFixture]
  public class MissionsApiControllerTests
  {
    [Test]
    public void GetResponderEmails_EmptyRoster()
    {
      var responders = new InMemoryDbSet<MissionRoster>();

      var dataMock = new Mock<IKcsarContext>();
      dataMock.SetupGet(f => f.MissionRosters).Returns(responders);

      var controller = new MissionsController(dataMock.Object, new AlwaysYesAuth(), new ConsoleLogger());

      var result = controller.GetResponderEmails(Guid.NewGuid(), null);
      Assert.IsEmpty(result);
    }

    [Test]
    public void ActionPermissions()
    {
      Type t = typeof(MissionsController);

      var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(f => f.GetCustomAttribute<HttpGetAttribute>() != null);

      foreach (var method in methods)
      {
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.IsNotNull(authorize, "Missing Authorize attribute");
      }
    }
  }
}
