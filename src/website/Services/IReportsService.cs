/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System.IO;
  using Kcsar.Database.Data;

  public interface IReportsService
  {
    Stream GetMissionReadyList(UnitRow unit);
    Stream GetMembershipReport();
  }
}
