namespace Sar.Database.Api.Extensions
{
  using System.Collections.Generic;
  using Kcsar.Database.Model;

  [ExtensionInterface]
  public interface IMissionReadyPlugin
  {
    IEnumerable<string> GetHeadersAfter(MissionReadyColumns column);
    IEnumerable<string> GetColumnsAfter(MissionReadyColumns column, Member member);
  }

  public enum MissionReadyColumns
  {
    Start = 0,
    WorkerNumber = 1,
    Name = 2,
    WorkerType = 3,
    Courses = 4,
    End = 99
  }
}