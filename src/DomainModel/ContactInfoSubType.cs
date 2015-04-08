/*
 * Copyright 2009-2015 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System.Collections.Generic;

  public class ContactInfoSubType
  {
    public string ValueLabel { get; set; }
    public string ValidationString { get; set; }
    public string[] SubTypes { get; set; }
  }
}
