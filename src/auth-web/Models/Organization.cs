using System;

namespace Sar.Database.Web.Auth
{
  public class Organization
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LongName { get; set; }

  }

  public class OrganizationMembership
  {
    public Organization Org { get; set; }
    public string Status { get; set; }
  }
}
