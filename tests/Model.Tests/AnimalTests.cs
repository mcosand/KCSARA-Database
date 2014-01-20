/*
 * Copyright 2014 Matthew Cosand
 */
namespace Internal.Database.Model
{
  using Kcsar.Database.Model;
  using NUnit.Framework;

  [TestFixture]
  public class AnimalTests
  {
    [Test]
    public void Initialization()
    {
      Animal animal = new Animal();
      Assert.AreEqual(Animal.AllowedTypes[0], animal.Type, "type");
      Assert.IsEmpty(animal.Owners, "owners");
      Assert.IsEmpty(animal.MissionRosters, "missionrosters");
    }

    [Test]
    public void GetPrimaryOwner()
    {
      var owner = new Member { FirstName = "Mary", LastName = "Smith" };
      var animal = new Animal();

      animal.Owners.Add(new AnimalOwner { Animal = animal, Owner = new Member { FirstName = "Joe", LastName = "Smith" } });
      animal.Owners.Add(new AnimalOwner { Animal = animal, Owner = owner, IsPrimary = true });

      var primary = animal.GetPrimaryOwner();
      Assert.AreSame(owner, primary);
    }

    [Test]
    public void GetPrimaryOwner_none()
    {
      var animal = new Animal();
      Assert.IsNull(animal.GetPrimaryOwner());
    }

    [Test]
    public void Report()
    {
      var animal = new Animal { Name = "testName", DemSuffix = "H", Comments = "Comments go here" };
      Assert.AreEqual(string.Format(Animal.ReportFormat, animal.Name, animal.DemSuffix, animal.Type, animal.Comments), animal.GetReportHtml());
    }

    [Test]
    public void Validate()
    {
      var animal = GetValidAnimal();
      Assert.IsTrue(animal.Validate());
    }

    [TestCase(null)]
    [TestCase("")]
    public void Validate_Name(string name)
    {
      var animal = GetValidAnimal();
      animal.Name = name;
      Assert.IsFalse(animal.Validate());
      Assert.AreEqual("Name", animal.Errors[0].PropertyName);
    }

    [TestCase(null)]
    [TestCase("")]
    public void Validate_Suffix(string suffix)
    {
      var animal = GetValidAnimal();
      animal.DemSuffix = suffix;
      Assert.IsFalse(animal.Validate());
      Assert.AreEqual("DemSuffix", animal.Errors[0].PropertyName);
    }

    [TestCase(null)]
    [TestCase("")]
    public void Validate_Type_Empty(string type)
    {
      var animal = GetValidAnimal();
      animal.Type = type;
      Assert.IsFalse(animal.Validate());
      Assert.AreEqual("Type", animal.Errors[0].PropertyName);
    }

    [Test]
    public void Validate_Type_Other()
    {
      var animal = GetValidAnimal();
      animal.Type = "other";
      Assert.IsFalse(animal.Validate());
      Assert.AreEqual("Type", animal.Errors[0].PropertyName);
      Assert.AreNotEqual("Required", animal.Errors[0].ErrorMessage);
    }

    private Animal GetValidAnimal()
    {
      return new Animal { Name = "TestName", DemSuffix = "H" };
    }
  }
}
