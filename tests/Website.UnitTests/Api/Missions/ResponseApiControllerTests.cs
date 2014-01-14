namespace Internal.Website.Api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Http;
  using System.Web.Http;
  using System.Web.Http.Controllers;
  using Kcsara.Database.Web.api.Models;
  using Kcsara.Database.Web.Areas.Missions;
  using Kcsara.Database.Web.Areas.Missions.api;
  using Moq;
  using NUnit.Framework;
  using M = Kcsar.Database.Model;

  [TestFixture]
  public class ResponseApiControllerTests
  {
    [Test]
    public void GetCurrentStatus_Empty()
    {
      var missions = new InMemoryDbSet<M.Mission>();
      var dataMock = new Mock<M.IKcsarContext>();
      dataMock.SetupGet(f => f.Missions).Returns(missions);

      var controller = new ResponseApiController(dataMock.Object, new AlwaysYesAuth(), new ConsoleLogger());

      MissionResponseStatus[] result = controller.GetCurrentStatus();
      Assert.IsEmpty(result);
    }

    [Test]
    public void GetCurrentStatus()
    {
      var missions = new InMemoryDbSet<M.Mission>() {
        new M.Mission { Title = "old", Location="f", StartTime=DateTime.Now.AddDays(-10), Roster = new List<M.MissionRoster>()},
        new M.Mission
        {
          Title = "First",
          Location = "Nowhere",
          StartTime = DateTime.Now.AddHours(-15)
        },
        new M.Mission{
          Title = "Third",
          Location = "nowhere",
          StartTime = DateTime.Now.AddHours(1),
          ResponseStatus = new M.MissionResponseStatus {
            CallForPeriod = DateTime.Now.AddHours(1),
          }
        }
      };

      var dataMock = new Mock<M.IKcsarContext>();
      dataMock.SetupGet(f => f.Missions).Returns(missions);

      var controller = new ResponseApiController(dataMock.Object, new AlwaysYesAuth(), new ConsoleLogger());

      MissionResponseStatus[] result = controller.GetCurrentStatus();
      Assert.AreEqual(2, result.Length, "expected count");
    }

    [Test]
    public void Create()
    {
      var missions = new InMemoryDbSet<M.Mission>();
      var dataMock = new Mock<M.IKcsarContext>();
      dataMock.SetupGet(f => f.Missions).Returns(missions);
      dataMock.Setup(f => f.SaveChanges()).Returns(1).Verifiable();

      //Mock<System.Web.Http.Routing.UrlHelper> urlHelper = new Mock<System.Web.Http.Routing.UrlHelper>(MockBehavior.Strict);
      //urlHelper.Setup(f => f.Route(MissionsAreaRegistration.RouteName, It.IsAny<object>()))
      //  .Callback<string,object>((a,b) => {
      //    Assert.AreEqual("Response", b.GetType().GetProperty("controller").GetValue(b), "routed controller");
      //  })
      //  .Returns("/blah/foo")
      //  .Verifiable();

      //Mock<HttpRequestContext> mockContext = new Mock<HttpRequestContext>(MockBehavior.Strict);
      //mockContext.SetupGet(f => f.Url).Returns(urlHelper.Object);
      
      var controller = new ResponseApiController(dataMock.Object, new AlwaysYesAuth(), new ConsoleLogger());
      //controller.RequestContext = mockContext.Object;
      //controller.Request = new HttpRequestMessage(HttpMethod.Post, "http://test/api/foo");

      CreateMission args = new CreateMission
      {
        Title = "Test Mission",
        Location = "test",
        Started = DateTime.Now
      };

      MissionResponseStatus result = controller.Create(args);
      
      dataMock.VerifyAll();
      Assert.AreEqual(1, missions.Count(), "length");
      Assert.AreEqual(missions.Single().Id, result.MissionId, "mission id");
      Assert.IsTrue(result.ShouldCall, "should call");
      Assert.IsTrue(result.ShouldStage, "should stage");
      Assert.AreEqual(missions.Single().Title, result.Title, "title");
    }

    [Test]
    public void Create_TooEarly()
    {
      var controller = new ResponseApiController(null, new AlwaysYesAuth(), new ConsoleLogger());
      CreateMission args = new CreateMission
      {
        Title = "Test Mission",
        Location = "test",
        Started = DateTime.Now.AddMinutes(-61)
      };

      var error = Assert.Throws<HttpResponseException>(() => controller.Create(args));
    }
  }
}
