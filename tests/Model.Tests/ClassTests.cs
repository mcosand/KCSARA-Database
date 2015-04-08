/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System;
using System.Linq;
using System.Reflection;
using Kcsar.Database.Data;
using Kcsar.Database.Data.Events;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Internal.Database.Data
{
  [TestFixture]
  public class ClassTests
  {
    private static Type[] IgnoredTypes = new[] { typeof(ReportingAttribute), typeof(MemberReportingAttribute) };

    [Test]
    public void NavigationPropertiesAreVirtual()
    {
      Assembly codefirstAssembly = typeof(KcsarContext).Assembly;

      foreach (Type type in codefirstAssembly.GetExportedTypes().OrderBy(f => f.Name))
      {
        if (type.IsInterface) continue;

        foreach (PropertyInfo propInfo in type.GetProperties().OrderBy(f => f.Name))
        {
          if (propInfo.PropertyType.Assembly != codefirstAssembly) continue;
          if (propInfo.PropertyType.IsEnum) continue;
          if (IgnoredTypes.Contains(propInfo.PropertyType)) continue;

          Assert.IsTrue(propInfo.GetMethod.IsVirtual, "{0}::get_{1} not virtual", type.Name, propInfo.Name);
          Console.WriteLine("{0}::{1}", type.Name, propInfo.Name);
        }
      }
    }

    [Test]
    public void MyTest()
    {
      using (var db = new KcsarContext("Data Source=(localdb)\\ProjectsV12;Initial Catalog=model_schema;Persist Security Info=True;Integrated Security=true;MultipleActiveResultSets=true", s => Console.WriteLine(s)))
      {
        var evt = new TrainingRow { Title = "Test event", StartTime = DateTime.Now };
        db.Events.Add(evt);

        var participant = new ParticipantRow { Firstname = "Joe", Lastname = "Bloe" };
        evt.Participants.Add(participant);

        evt.Timeline.Add(new EventLogRow { Time = DateTime.Now, Participant = participant, JsonData = JsonConvert.SerializeObject("This is a test message") });
        db.SaveChanges();

      }
    }
  }
}
