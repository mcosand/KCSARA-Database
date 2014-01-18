/*
 * Copyright 2014 Matthew Cosand
 */
namespace ESAR.Extensions
{
  using System.Collections.Generic;
  using Kcsara.Database.Services.Reports;

  public class MissionReadyExtension : IMissionReadyPlugin
  {
    public IEnumerable<string> GetColumnsAfter(MissionReadyColumns column, Kcsar.Database.Model.Member member)
    {
      if (column == MissionReadyColumns.WorkerType)
      {
        return new[] { "foo" };
      }
      return new string[0];
    }

    public IEnumerable<string> GetHeadersAfter(MissionReadyColumns column)
    {
      if (column == MissionReadyColumns.WorkerType)
      {
        return new[] { "Rank" };
      }
      return new string[0];
    }
  }
}
