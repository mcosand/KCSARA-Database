/*
 * Copyright 2016 Matthew Cosand
 */
using System;

namespace Kcsara.Database.Model.Training
{
  public class ParsedKcsaraCsv
  {
    public string Link { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public NameIdPair Member { get; set; }
    public NameIdPair Course { get; set; }

    public DateTime Completed { get; set; }

    public Guid? Existing { get; set; }
    public string Error { get; set; }
  }
}
