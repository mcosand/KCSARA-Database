/*
 * Copyright 2014 Matthew Cosand
 */
namespace Internal.Database.Data
{
  using System.Linq;
  using Kcsar.Database.Data;
  using NUnit.Framework;

  [TestFixture]
  public class AnimalMissionTests
  {
    [Test]
    public void Validate()
    {
      var am = new AnimalEventRow();
      Assert.AreEqual(0, am.Validate(null).Count());
    }
  }
}
