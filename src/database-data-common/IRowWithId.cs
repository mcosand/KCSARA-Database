using System;

namespace Sar.Database.Data
{
  public interface IRowWithId
  {
    Guid Id { get; }
  }
}
