using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Internal.Data.Model;
using Moq;
using NUnit.Framework;
using Sar;
using Sar.Database.Model;
using Sar.Database.Model.Members;
using Sar.Database.Model.Units;
using Sar.Database.Services;

namespace database_services_tests
{
  [TestFixture]
  public class AuthorizationTests
  {
    private ClaimsPrincipal GetNonMemberUser()
    {
      return new ClaimsPrincipal(
        new ClaimsIdentity(
          new[] {
            new Claim("login", "login"),
            new Claim("sub", Guid.NewGuid().ToString()),
          },
          "test"));
    }

    private IHost GetHost(ClaimsPrincipal user)
    {
      var hostMock = new Mock<IHost>(MockBehavior.Strict);
      hostMock.SetupGet(f => f.User).Returns(user);
      return hostMock.Object;
    }

    private static IRolesService MockRoles(IHost host, IEnumerable<string> roles)
    {
      var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
      var subject = host.User.GetSubject();
      rolesService.Setup(f => f.RolesForAccount(subject)).Returns(roles.ToList());
      return rolesService.Object;
    }

    public void CreateTrainingRecord(string addRoles, bool canCreate)
    {
      ClaimsPrincipal user = GetNonMemberUser();
      List<string> roles = (addRoles ?? string.Empty).Split(',').ToList();
      roles.Add("cdb.users");

      var db = new FakeKcsarContext();

      var host = GetHost(user);
      var authz = new AuthorizationService(() => db.Mock.Object, host, MockRoles(host, roles));

      Assert.AreEqual(canCreate, authz.CanCreate)
    }

    [Test]
    [TestCase("", true, false, TestName = "Create_Any_UnitStatusType_User")]
    [TestCase("cdb.test.admins", true, false, TestName = "Create_Any_UnitStatusType_UnitAdmin")]
    [TestCase("cdb.admins", true, true, TestName = "Create_Any_UnitStatusType_Admin")]
    [TestCase("", false, false, TestName = "Create_Unit_UnitStatusType_User")]
    [TestCase("cdb.test.admins", false, true, TestName = "Create_Unit_UnitStatusType_UnitAdmin")]
    [TestCase("cdb.admins", false, true, TestName = "Create_Unit_UnitStatusType_Admin")]
    public void CreateUnitStatusType(string addRoles, bool globalCreate, bool canCreate)
    {
      ClaimsPrincipal user = GetNonMemberUser();
      List<string> roles = (addRoles ?? string.Empty).Split(',').ToList();
      roles.Add("cdb.users");

      var unitRow = new Kcsar.Database.Model.SarUnit { DisplayName = "Test" };

      var db = new FakeKcsarContext();
      db.Units.Add(unitRow);

      var host = GetHost(user);
      var authz = new AuthorizationService(() => db.Mock.Object, host, MockRoles(host, roles));

      Assert.AreEqual(canCreate, authz.CanCreateStatusForUnit(globalCreate ? (Guid?)null : unitRow.Id));
    }

    [Test]
    [TestCase("", false, false, TestName = "WrapUnitStatusType_User")]
    [TestCase("cdb.test.admins", true, true, TestName = "WrapUnitStatusType_UnitAdmin")]
    [TestCase("cdb.admins", true, true, TestName = "WrapUnitStatusType_Admin")]
    public void WrapUnitStatusType(string addRoles, bool canUpdate, bool canDelete)
    {
      ClaimsPrincipal user = GetNonMemberUser();
      List<string> roles = (addRoles ?? string.Empty).Split(',').ToList();
      roles.Add("cdb.users");

      var unitRow = new Kcsar.Database.Model.SarUnit { DisplayName = "Test" };

      var db = new FakeKcsarContext();
      db.Units.Add(unitRow);

      var host = GetHost(user);
      var authz = new AuthorizationService(() => db.Mock.Object, host, MockRoles(host, roles));
      var status = new UnitStatusType
      {
        Unit = new NameIdPair { Id = unitRow.Id, Name = unitRow.DisplayName },
        Name = "active",
        WacLevel = WacLevel.Support
      };

      var wrapped = authz.Wrap(status);

      Assert.AreEqual(canDelete, wrapped.D, "delete");
      Assert.AreEqual(canUpdate, wrapped.U, "update");
    }

    [Test]
    [TestCase("", false, false, TestName = "Create_Any_UnitMembership_User")]
    [TestCase("cdb.test.admins", true, false, TestName = "Create_Any_UnitMembership_UnitAdmin")]
    [TestCase("cdb.admins", true, true, TestName = "Create_Any_UnitMembership_Admin")]
    [TestCase("", false, false, TestName = "Create_Unit_UnitMembership_User")]
    [TestCase("cdb.test.admins", false, true, TestName = "Create_Unit_UnitMembership_UnitAdmin")]
    [TestCase("cdb.admins", false, true, TestName = "Create_Unit_UnitMembership_Admin")]
    public void CreateUnitMembership(string addRoles, bool globalCreate, bool canCreate)
    {
      ClaimsPrincipal user = GetNonMemberUser();
      List<string> roles = (addRoles ?? string.Empty).Split(',').ToList();
      roles.Add("cdb.users");

      var unitRow = new Kcsar.Database.Model.SarUnit { DisplayName = "Test" };

      var db = new FakeKcsarContext();
      db.Units.Add(unitRow);

      var host = GetHost(user);
      var authz = new AuthorizationService(() => db.Mock.Object, host, MockRoles(host, roles));

      Assert.AreEqual(canCreate, authz.CanCreateMembershipForUnit(globalCreate ? (Guid?)null : unitRow.Id));
    }

    [Test]
    [TestCase("", false, false, TestName = "WrapUnitMembership_User")]
    [TestCase("cdb.test.admins", true, true, TestName = "WrapUnitMembership_UnitAdmin")]
    [TestCase("cdb.admins", true, true, TestName = "WrapUnitMembership_Admin")]
    public void WrapUnitMembership(string addRoles, bool canUpdate, bool canDelete)
    {
      ClaimsPrincipal user = GetNonMemberUser();
      List<string> roles = (addRoles ?? string.Empty).Split(',').ToList();
      roles.Add("cdb.users");

      var unitRow = new Kcsar.Database.Model.SarUnit { DisplayName = "Test" };
      var memberRow = new Kcsar.Database.Model.Member { FirstName = "Blay", LastName = "User" };

      var db = new FakeKcsarContext();
      db.Units.Add(unitRow);
      db.Members.Add(memberRow);

      var host = GetHost(user);
      var authz = new AuthorizationService(() => db.Mock.Object, host, MockRoles(host, roles));
      var status = new UnitMembership
      {
        Unit = new NameIdPair { Id = unitRow.Id, Name = unitRow.DisplayName },
        Member = new MemberSummary { Id = memberRow.Id, Name = "Blah" }
      };

      var wrapped = authz.Wrap(status);

      Assert.AreEqual(canDelete, wrapped.D, "delete");
      Assert.AreEqual(canUpdate, wrapped.U, "update");
    }
    
    [Test]
    [TestCase("", false, TestName = "CreateUnit_User")]
    [TestCase("cdb.test.admins", false, TestName = "CreateUnit_UnitAdmin")]
    [TestCase("cdb.admins", true, TestName = "CreateUnit_Admin")]
    public void CreateUnit(string addRoles, bool canCreate)
    {
      ClaimsPrincipal user = GetNonMemberUser();
      List<string> roles = (addRoles ?? string.Empty).Split(',').ToList();
      roles.Add("cdb.users");

      var db = new FakeKcsarContext();

      var host = GetHost(user);
      var authz = new AuthorizationService(() => db.Mock.Object, host, MockRoles(host, roles));

      Assert.AreEqual(canCreate, authz.CanCreate<Unit>());
    }

    [Test]
    [TestCase("", false, false, TestName = "WrapUnit_User")]
    [TestCase("cdb.test.admins", true, true, TestName = "WrapUnit_UnitAdmin")]
    [TestCase("cdb.admins", true, true, TestName = "WrapUnit_Admin")]
    public void WrapUnit(string addRoles, bool canUpdate, bool canDelete)
    {
      ClaimsPrincipal user = GetNonMemberUser();

      List<string> roles = (addRoles ?? string.Empty).Split(',').ToList();
      roles.Add("cdb.users");

      var unitRow = new Kcsar.Database.Model.SarUnit { DisplayName = "Test" };

      var db = new FakeKcsarContext();
      db.Units.Add(unitRow);

      var host = GetHost(user);
      var authz = new AuthorizationService(() => db.Mock.Object, host, MockRoles(host, roles));
      var status = new Unit
      {
        Id = unitRow.Id,
        Name = "Test",
        County = "king",
        FullName = "Test Rescue Group"
      };

      var wrapped = authz.Wrap(status);

      Assert.AreEqual(canDelete, wrapped.D, "delete");
      Assert.AreEqual(canUpdate, wrapped.U, "update");
    }
  }
}
