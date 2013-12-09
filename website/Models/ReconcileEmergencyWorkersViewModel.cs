using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    public class ReconcileEmergencyWorkersRow
    {
        public string Name { get; set; }
        public string DEM { get; set; }
        public Guid Id { get; set; }
        public int MissionCount { get; set; }
        public int TrainingCount { get; set; }

        public bool DemIsNew { get; set; }
        public bool WacIsNew { get; set; }
        public string WacLevel { get; set; }
        public string UnitStatus { get; set; }
        public bool IsOldNovice { get; set; }
        public bool? TrainingCurrent { get; set; }
    }

    public class ReconcileEmergencyWorkersViewModel
    {
        public ReconcileEmergencyWorkersViewModel()
        {
            NoMatch = new List<string>();
            MembersByUnit = new Dictionary<string, List<ReconcileEmergencyWorkersRow>>();
        }

        public List<string> NoMatch { get; set; }
        public Dictionary<string, List<ReconcileEmergencyWorkersRow>> MembersByUnit { get; set; }
        public int RowCount { get; set; }
    }
}