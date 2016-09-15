using System;
using System.Collections.Generic;
using System.Linq;

namespace Sar.Database.Web.Auth
{
  public class Member
  {
    public Member()
    {
      Units = new List<OrganizationMembership>();
    }

    public Guid Id { get; set; }

    public string Email { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhotoUrl { get; set; }
    public string ProfileUrl { get; set; }


    public bool IsActive {  get { return Units != null && Units.Count() > 0;  } }

    public IEnumerable<OrganizationMembership> Units { get; set; }
  }
}
