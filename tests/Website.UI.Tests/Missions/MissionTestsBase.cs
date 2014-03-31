namespace Internal.Website.Missions
{
  using System;
  using System.Linq;
  using Kcsar.Database.Model;

  public abstract class MissionTestsBase : BrowserFixture
  {
    protected Guid MissionId { get; private set; }

    public override void FixtureSetup()
    {
      base.FixtureSetup();

      using (var db = context.GetDb())
      {
        Mission m = new Mission
        {
          Title = "Mission " + Guid.NewGuid().ToString(),
          Location = "somewhere",
          StartTime = DateTime.Today.AddHours(3)
        };
        db.Missions.Add(m);
        db.SaveChanges();
        this.MissionId = m.Id;
      }
    }

    public override void FixtureTeardown()
    {
      base.FixtureTeardown();
      using (var db = context.GetDb())
      {
        var a = db.Missions.SingleOrDefault(f => f.Id == this.MissionId);
        if (a != null)
        {
          foreach (var roster in db.MissionRosters.Where(f => f.Mission.Id == this.MissionId))
            db.MissionRosters.Remove(roster);
        }
        db.Missions.Remove(a);
        db.SaveChanges();
      }
    }
  }
}
