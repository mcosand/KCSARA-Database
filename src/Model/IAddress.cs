/*
 * Copyright 2010-2015 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using Microsoft.SqlServer.Types;

  public interface IAddress
  {
    string Street { get; set; }
    string City { get; set; }
    string State { get; set; }
    string Zip { get; set; }
  }

  public interface IRefinableAddress : IAddress
  {
    int Quality { get; set; }
  }

  public interface IAddressGeography : IRefinableAddress
  {
    SqlGeography Location { get; set; }
  }
}
