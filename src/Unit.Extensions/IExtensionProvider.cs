/*
 * Copyright 2014-2015 Matthew Cosand
 */
namespace Kcsara.Database.Extensions
{
  using Kcsar.Database.Data;

  [ExtensionInterface]
  public interface IExtensionProvider
  {
    void Initialize();
    T For<T>(UnitRow unit);
  }
}
