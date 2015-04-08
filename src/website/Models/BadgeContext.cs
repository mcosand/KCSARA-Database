/*
 * Copyright 2008-2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Model
{
  using Kcsar.Database.Data;

  public class BadgeContext
    {
        public MemberRow Member { get; set; }
        public bool IsPassport { get; set; }
    }
}
