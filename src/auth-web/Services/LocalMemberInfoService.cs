using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kcsara.Database.Model.Members;
using Kcsara.Database.Services;
using Kcsara.Database.Services.Members;
using Sar.Services;

namespace Sar.Auth.Services
{
  class LocalMemberInfoService : IMemberInfoService
  {
    private readonly IHost _host;
    private readonly IMembersService _members;
    private readonly IUnitsService _units;

    public LocalMemberInfoService(IMembersService members, IUnitsService units, IHost host)
    {
      _members = members;
      _units = units;
      _host = host;
    }

    public async Task<IList<MemberInfo>> FindMembersByEmail(string email)
    {
      var list = new List<MemberInfo>();
			
      foreach (var member in (await _members.ByEmail(email)))
      {
        list.Add(await _members.GetMember(member.Id));
      }

      return list;
    }

    public async Task<Member> GetMember(Guid memberId)
    {
      var info = await _members.GetMember(memberId);

      var units = (await _units.List()).ToDictionary(f => f.Id, f => new Organization {
        Id = f.Id,
        Name = f.Name,
        LongName = f.FullName
      });

      var email = (await _members.ListMemberContactsAsync(memberId)).Where(f => f.Type == "email").FirstOrDefault();

      return new Member
      {
        Units = info.Units.Select(f => new OrganizationMembership { Org = units[f.Unit.Id], Status = f.Status }),
        Id = info.Id,
        FirstName = info.First,
        LastName = info.Last,
        Email = email?.Value,
        PhotoUrl = info.Photo,
        ProfileUrl = string.Format(_host.GetConfig("memberProfileTemplate"), info.Id)
      };
    }

    public async Task<Dictionary<string, bool>> GetStatusToAccountMap()
    {
      return (await _units.ListStatusTypes()).ToDictionary(f => (f.Unit.Id.ToString() + f.Name).ToLowerInvariant(), f => f.GetsAccount);
    }
  }
}
