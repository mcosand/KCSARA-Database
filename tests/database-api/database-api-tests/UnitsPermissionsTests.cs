using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kcsara.Database.Api.Controllers.Units;
using Moq;
using NUnit.Framework;
using Sar;
using Sar.Database.Services;

namespace database_api_tests
{
  [TestFixture]
  public class UnitsPermissionsTests
  {
    [Test]
    public void ListUnitsChecksAuth()
    {
      string policy = "Read:Unit";
      Guid? reference = null;


      var serviceMock = new Mock<IUnitsService>(MockBehavior.Strict);
      var authMock = new Mock<IAuthorizationService>(MockBehavior.Strict);

      Expression<Func<IAuthorizationService, Task<bool>>> authCall = f => f.EnsureAsync(reference, policy, true, null);
      authMock.Setup(authCall).Throws(new AuthorizationException()).Verifiable();

      var controller = new UnitsController(serviceMock.Object, authMock.Object);
      Assert.Throws<AuthorizationException>(async () => await controller.List());

      authMock.Verify(authCall, Times.Once);
    }
  }
}
