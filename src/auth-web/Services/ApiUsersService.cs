using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Sar.Auth.Data;
using Sar.Database.Services;

namespace Sar.Database.Web.Auth.Services
{
  public class ApiUsersService : IUsersService
  {
    private readonly Func<IAuthDbContext> _dbFactory;
    private readonly IHost _host;

    public ApiUsersService(Func<IAuthDbContext> dbFactory, IHost host)
    {
      _host = host;
      _dbFactory = dbFactory;
    }

    public Task<User> GetCurrentUser()
    {
      var subClaim = _host.User.FindFirst("sub")?.Value;
      Guid sub;
      if (string.IsNullOrWhiteSpace(subClaim) || !Guid.TryParse(subClaim, out sub)) return null;

      return GetUser(sub);
    }

    public async Task<User> GetUser(Guid id)
    {
      using (var db = _dbFactory())
      {
        return await db.Accounts.Where(f => f.Id == id).Select(f => new User
        {
          Id = f.Id,
          Username = f.Username,
          FirstName = f.FirstName,
          LastName = f.LastName,
          Email = f.Email
        }).FirstOrDefaultAsync();
      }
    }
  }
}
