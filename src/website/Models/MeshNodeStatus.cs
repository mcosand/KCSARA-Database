/*
 * Copyright 2011-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Spatial;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kcsara.Database.Web.Model
{
    public class MeshNodeStatus
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public DbGeography Location { get; set; }
        public string IPAddr { get; set; }
        public float Uptime { get; set; }
        public float BatteryVolts { get; set; }
        public float HouseVolts { get; set; }
        public float AlternatorVolts { get; set; }
    }

    public class MeshNodeLocation
    {
        [Key, Column(Order=1)]
        public string Name { get; set; }

        [Key, Column(Order=2)]
        public DateTime Time { get; set; }

        [Required]
        public DbGeography Location { get; set; }
    }

    public class MeshNodeEntities : DbContext
    {
        //public MeshNodeEntities()
        //    : base("name=MeshNodeData")
        //{
        //}

        public DbSet<MeshNodeStatus> Checkins { get; set; }
        public DbSet<MeshNodeLocation> Locations { get; set; }
    }
}
