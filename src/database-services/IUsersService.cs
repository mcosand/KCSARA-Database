using System;
using System.Threading.Tasks;

namespace Sar.Database.Services
{
  public interface IUsersService
  {
    Task<User> GetUser(Guid id);
    Task<User> GetCurrentUser();
  }
}
