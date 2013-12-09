
namespace Kcsar.Database.Model
{
    using System;
    //using System.Data.Objects.DataClasses;
    using System.Collections.Generic;

    public interface IRosterEvent<E, R> : IRosterEvent
        where R : class, IRosterEntry<E, R>
        where E : IRosterEvent<E, R>
    {
     //   EntityCollection<R> Roster { get; }
        IEnumerable<R> Roster { get; }
    }    

    public interface IRosterEntry<E, R> : IRosterEntry where E : IRosterEvent<E, R> where R : class, IRosterEntry<E, R>
    {
        IRosterEvent<E, R> GetRosterEvent();
    }

    public interface IRosterEvent : IModelObject
    {
        string Title { get; set; }
        string Location { get; set; }
        string StateNumber { get; set; }
        DateTime StartTime { get; set; }
        DateTime? StopTime { get; set; }
        string County { get; set; }
    }

    public interface IRosterEntry : IModelObject
    {
        Member Person { get; set; }
        DateTime? TimeIn { get; set; }
        DateTime? TimeOut { get; set;  }
        int? Miles { get; set; }
        double? Hours { get; }
        IRosterEvent GetEvent();
        void SetEvent(IRosterEvent sarEvent);
    }
}
