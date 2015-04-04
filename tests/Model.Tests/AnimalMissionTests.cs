/*
 * Copyright 2014 Matthew Cosand
 */
namespace Internal.Database.Model
{
  using System;
  using System.Linq;
  using Kcsar.Database.Model;
  using Kcsar.Database.Model.Events;
  using NUnit.Framework;

  [TestFixture]
  public class AnimalMissionTests
  {
    [Test]
    public void Validate()
    {
      var am = new AnimalEvents();
      Assert.AreEqual(0, am.Validate(null).Count());
    }
  }
}
