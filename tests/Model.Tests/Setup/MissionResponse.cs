using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Kcsar.Database.Model;
using NUnit.Framework;

namespace Internal.Database.Model.Setup
{
  [TestFixture]
  public class MissionResponse
  {
    [Test]
    [Explicit]
    public void MissionResponseData()
    {
      var db = ContextGenerator.CreateContext();
      MockMissions.Create(db);
      Console.WriteLine(db.SaveChanges());
    }
  }
}
