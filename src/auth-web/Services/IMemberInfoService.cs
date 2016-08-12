/*
 * Copyright Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kcsara.Database.Model.Members;

namespace Sar.Auth
{
  public interface IMemberInfoService
  {
    Task<IList<MemberInfo>> FindMembersByEmail(string email);
    Task<Member> GetMember(Guid memberId);
  }
}
