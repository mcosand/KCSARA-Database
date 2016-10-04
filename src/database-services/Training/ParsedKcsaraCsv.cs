using System;
using Sar.Database.Model;

namespace Sar.Database.Services.Training
{
  public class ParsedKcsaraCsv
  {
    public string Link { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public NameIdPair Member { get; set; }
    public NameIdPair Course { get; set; }

    public DateTimeOffset Completed { get; set; }

    public Guid? Existing { get; set; }
    public string Error { get; set; }
  }
}
