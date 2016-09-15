using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sar.Database.Model.Members;

namespace Sar.Database.Web.Auth
{
  public interface IMemberInfoService
  {
    Task<IList<MemberInfo>> FindMembersByEmail(string email);
    Task<Member> GetMember(Guid memberId);
    Task<Dictionary<string, bool>> GetStatusToAccountMap();
  }
}
