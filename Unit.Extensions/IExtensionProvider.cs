/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  [ExtensionInterface]
  public interface IExtensionProvider
  {
    void Initialize();
    T For<T>(object key);
  }
}
