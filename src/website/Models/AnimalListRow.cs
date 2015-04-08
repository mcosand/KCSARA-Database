/*
 * Copyright 2009-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.Model
{
  using System;
  using Kcsar.Database.Data;

  public class AnimalListRow
    {
        public AnimalRow Animal { get; set; }
        public string PrimaryOwnerName { get; set; }
        public MemberRow PrimaryOwner { get; set; }
        public DateTime? ActiveUntil { get; set; }
    }
}
