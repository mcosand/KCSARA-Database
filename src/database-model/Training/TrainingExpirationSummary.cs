/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsar.Database.Model
{
    public class TrainingExpirationSummary
    {
        [Key]
        public Guid CourseId { get; set; }
        public int Expired { get; set; }
        public int Recent { get; set; }
        public int Almost { get; set; }
        public int Good { get; set; }
    }
}
