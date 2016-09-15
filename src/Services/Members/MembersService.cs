using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kcsara.Database.Web.Model;
using Kcsara.Database.Web.Services.ObjectModel.Members;
using Data = Kcsar.Database.Model;

namespace Kcsara.Database.Web.Services.Members
{
  public interface IMembersService
  {
    Task<IEnumerable<MemberSummary>> ByPhoneNumber(string id);
    Task<IEnumerable<MemberSummary>> ByWorkerNumber(string id);
  }

  public class MembersService : IMembersService
  {
    private readonly Func<Data.IKcsarContext> _dbFactory;

    public MembersService(Func<Data.IKcsarContext> dbFactory)
    {
      _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<MemberSummary>> ByWorkerNumber(string id)
    {
      id = id.TrimStart('S', 'R');

      using (var db = _dbFactory())
      {
        return await SummariesWithUnits(db.Members.Where(f => f.DEM == id || f.DEM == "SR" + id));
      }
    }

    public async Task<IEnumerable<MemberSummary>> ByPhoneNumber(string id)
    {
      if (id.Length < 10 || !Regex.IsMatch(id, "\\d+"))
      {
        return new MemberSummary[0];
      }

      var pattern = string.Format("%{0}%{1}%{2}%",
        id.Substring(id.Length - 10, 3),
        id.Substring(id.Length - 7, 3),
        id.Substring(id.Length - 4, 4));

      using (var db = _dbFactory())
      {
        return await SummariesWithUnits(db.Members.Where(f => f.ContactNumbers.Any(g => SqlFunctions.PatIndex(pattern, g.Value) > 0)));
      }
    }

    internal static async Task<IEnumerable<MemberSummary>> SummariesWithUnits(IQueryable<Data.Member> query)
    {
      DateTime cutoff = DateTime.Now;
      return (await query
        .Select(f => new
        {
          Member = f,
          Units = f.Memberships
                   .Where(g => (g.EndTime == null || g.EndTime > cutoff) && g.Status.IsActive)
                   .Select(g => g.Unit)
                   .Select(g => new NameIdPair
                   {
                     Id = g.Id,
                     Name = g.DisplayName
                   }).Distinct()
        })
        .OrderBy(f => f.Member.LastName).ThenBy(f => f.Member.FirstName)
        .ToListAsync())
        .Select(f => new MemberSummary
        {
          Name = f.Member.FullName,
          WorkerNumber = f.Member.DEM,
          Id = f.Member.Id,
          Units = f.Units.ToArray(),
          Photo = f.Member.PhotoFile
        });
    } 
  }
}
