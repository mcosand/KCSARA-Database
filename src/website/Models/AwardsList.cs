/*
 * Copyright 2009-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.Model
{
  using System.Collections.Generic;
  using Kcsar.Database.Data;

  public class AwardsList
    {
        public MemberRow Member { get; set; }
        public IEnumerable<ITrainingAwardRow> Awards { get; set; }
    }
}
