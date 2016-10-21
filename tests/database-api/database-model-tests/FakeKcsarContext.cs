using System;
using System.Linq;
using Kcsar.Database.Model;

namespace Internal.Data.Model
{
  public class FakeKcsarContext : FakeDbContext<IKcsarContext>
  {
    public FakeDbSet<SarUnit> Units { get; private set; }
    public FakeDbSet<Member> Members { get; private set; }

    public FakeKcsarContext()
    {
      var dbSetProperties = typeof(FakeKcsarContext).GetProperties()
                                .Where(f => f.PropertyType.IsGenericType && f.PropertyType.GetGenericTypeDefinition() == typeof(FakeDbSet<>));

      foreach (var property in dbSetProperties)
      {
        var table = Activator.CreateInstance(property.PropertyType);
        // Assign the value using the private setter.
        var setter = property.GetSetMethod(true);
        setter.Invoke(this, new[] { table });
      }

      Init();
    }

    protected override void SetupMock()
    {
      base.SetupMock();

      // TODO - use reflection to make this generic.
      Mock.SetupGet(f => f.Units).Returns(Units);
      Mock.SetupGet(f => f.Members).Returns(Members);
    }
  }
}
