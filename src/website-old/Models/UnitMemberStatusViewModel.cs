/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System;

namespace Kcsara.Database.Web.Model
{
    public class UnitMemberStatusViewModel
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public bool IsCurrent { get; set; }
        public bool AdminRole { get; set; }
    }
}
