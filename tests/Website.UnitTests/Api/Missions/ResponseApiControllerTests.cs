namespace Internal.Website.Api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Http;
  using System.Web.Http;
  using System.Web.Http.Controllers;
  using Internal.Database.Model;
  using Kcsara.Database.Web.api.Models;
  using Kcsara.Database.Web.Areas.Missions;
  using Kcsara.Database.Web.Areas.Missions.api;
  using Kcsara.Database.Web.Controllers;
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

      var controller = new ResponseApiController(null, new ControllerArgs(dataMock.Object, new AlwaysYesAuth(), new ConsoleLogger(), null));

      MissionResponseStatus[] result = controller.GetCurrentStatus();
      Assert.IsEmpty(result);
    }

    [Test]
    public void GetCurrentStatus()
    {
      var dataMock = GetBasicResponseData();

      var controller = new ResponseApiController(null, new ControllerArgs(dataMock.Object, new AlwaysYesAuth(), new ConsoleLogger(), null));

      MissionResponseStatus[] result = controller.GetCurrentStatus();
      Assert.AreEqual(dataMock.Object.Missions.Count(), result.Length, "expected count");
    }

    public static Mock<M.IKcsarContext> GetBasicResponseData()
    {
      var missions = new InMemoryDbSet<M.Mission>(); // {
      //  new M.Mission { Title = "old", Location="f", StartTime=DateTime.Now.AddDays(-10), Roster = new List<M.MissionRoster>()},
      //  new M.Mission
      //  {
      //    Title = "First",
      //    Location = "Nowhere",
      //    StartTime = DateTime.Now.AddHours(-15)
      //  },
      //  new M.Mission{
      //    Title = "Third",
      //    Location = "nowhere",
      //    StartTime = DateTime.Now.AddHours(1),
      //    ResponseStatus = new M.MissionResponseStatus {
      //      CallForPeriod = DateTime.Now.AddHours(1),
      //    }
      //  }
      //};

      var dataMock = new Mock<M.IKcsarContext>();
      dataMock.SetupGet(f => f.Missions).Returns(missions);
      dataMock.SetupGet(f => f.UnitMemberships).Returns(new InMemoryDbSet<M.UnitMembership>());
      dataMock.SetupGet(f => f.UnitStatusTypes).Returns(new InMemoryDbSet<M.UnitStatus>());
      dataMock.SetupGet(f => f.Members).Returns(new InMemoryDbSet<M.Member>());
      dataMock.SetupGet(f => f.Units).Returns(new InMemoryDbSet<M.SarUnit>());
      MockMissions.Create(dataMock.Object);
      return dataMock;
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
      
      var controller = new ResponseApiController(null, new ControllerArgs(dataMock.Object, new AlwaysYesAuth(), new ConsoleLogger(), null));
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
      Assert.AreEqual(missions.Single().Id, result.Mission.Id, "mission id");
      Assert.IsTrue(result.ShouldCall, "should call");
      Assert.IsTrue(result.ShouldStage, "should stage");
      Assert.AreEqual(missions.Single().Title, result.Mission.Title, "title");
    }

    [Test]
    public void Create_TooEarly()
    {
      var controller = new ResponseApiController(null, new ControllerArgs(null, new AlwaysYesAuth(), new ConsoleLogger(), null));
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
