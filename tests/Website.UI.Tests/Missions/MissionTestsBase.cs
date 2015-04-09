namespace Internal.Website.Missions
{
  using System;
  using System.Linq;
  using Kcsar.Database.Data.Events;
  using Kcsar.Database.Model;

  public abstract class MissionTestsBase : BrowserFixture
  {
    protected Guid MissionId { get; private set; }

    public override void FixtureSetup()
    {
      base.FixtureSetup();

      using (var db = context.GetDb())
      {
        MissionRow m = new MissionRow
        {
          Title = "Mission " + Guid.NewGuid().ToString(),
          Location = "somewhere",
          StartTime = DateTime.Today.AddHours(3)
        };
        db.Events.Add(m);
        db.SaveChanges();
        this.MissionId = m.Id;
      }
    }

    public override void FixtureTeardown()
    {
      base.FixtureTeardown();
      using (var db = context.GetDb())
      {
        var a = db.Events.OfType<MissionRow>().SingleOrDefault(f => f.Id == this.MissionId);
        //if (a != null)
        //{
        //  foreach (var roster in db.MissionRosters.Where(f => f.Mission.Id == this.MissionId))
        //    db.Remove(roster);
        //}
        db.Events.Remove(a);
        db.SaveChanges();
      }
    }
  }
}
