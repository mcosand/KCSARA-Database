using System;
using System.Threading.Tasks;

namespace Kcsar.Database.Model
{
  /// <summary>
  /// An interface for the database context.
  /// </summary>
  public interface IDbContext : IDisposable
  {
    /// <summary>
    /// Saves changes.
    /// </summary>
    /// <returns>The number of rows modified.</returns>
    Task<int> SaveChangesAsync();
  }
}
