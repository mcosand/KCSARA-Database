/*
 * Copyright 2010-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Model
{
    using Kcsar.Database.Model;
    using System.Xml.Serialization;
    using System.Xml;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Microsoft.SqlServer.Types;
    using System.Runtime.Serialization;

    [DataContract(Namespace = "")]
    [KnownType(typeof(WaypointView))]
    [KnownType(typeof(RouteView))]
    [XmlInclude(typeof(WaypointView))]
    [XmlInclude(typeof(RouteView))]
    public abstract class GeographyView
    {
        public static class Kinds
        {
            public const string Mission_LKP = "cluLkp";
            public const string Member_Residence = "m_res";
        }

        [DataMember(EmitDefaultValue = false)]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember]
        public string Kind { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public Guid? InstanceId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? Time { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? EventId { get; set; }

        public static GeographyView BuildGeographyView(PersonAddress geog)
        {
            GeographyView view = BuildGeographyView(geog.Location);
            view.Id = geog.Id;
            view.Kind = Kinds.Member_Residence;
            view.Description = string.Format("{0}\n{1}\n{2}, {3}", geog.Person.FullName, geog.Street, geog.City, geog.Zip);
            view.InstanceId = geog.Person.Id;
            return view;
        }

        public static GeographyView BuildGeographyView(MissionGeography geog)
        {
            GeographyView view = BuildGeographyView(geog.Geography);
            view.Id = geog.Id;
            view.Kind = geog.Kind;
            view.Description = geog.Description;
            view.InstanceId = geog.InstanceId;
            return view;
        }

        private static GeographyView BuildGeographyView(SqlGeography geog)
        {
            GeographyView view;
            int dimensions = geog.STDimension().Value;

            if (dimensions == 0)
            {
                view = new WaypointView
                {
                    Lat = geog.Lat.Value,
                    Long = geog.Long.Value,
                    Z = geog.Z.IsNull ? (double?)null : geog.Z.Value,
                    Time = geog.M.IsNull ? (DateTime?)null : DateTime.FromOADate(geog.M.Value)
                };
            }
            else if (dimensions == 1)
            {
                view = BuildRouteViewGeography(geog);
            }
            else
            {
                throw new NotImplementedException();
            }
            return view;
        }

        private static RouteView BuildRouteViewGeography(SqlGeography geography)
        {
            RouteView view = new RouteView() { Length = geography.STLength().Value };

            view.Points = new List<MapCoordinate>();
            DateTime? minTime = null;
            DateTime? maxTime = null;
            int count = geography.STNumPoints().Value;

            for (int i = 1; i <= count; i++)
            {
                var geog = geography.STPointN(i);
                MapCoordinate c = new MapCoordinate
                {
                    Lat = geog.Lat.Value,
                    Long = geog.Long.Value,
                    Z = geog.Z.IsNull ? (double?)null : geog.Z.Value,
                    Time = geog.M.IsNull ? (DateTime?)null : DateTime.FromOADate(geog.M.Value)
                };
                view.Points.Add(c);
                if (c.Time < minTime || minTime == null)
                {
                    minTime = c.Time;
                }
                if (c.Time > maxTime || maxTime == null)
                {
                    maxTime = c.Time;
                }
            }
            view.Time = minTime;
            return view;
        }

        public abstract SqlGeography AsSqlGeography();
    }

    [DataContract(Namespace = "")]
    public class WaypointView : GeographyView
    {
        [DataMember]
        public double Lat {
            get;
            set;
        }

        [DataMember]
        public double Long { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double? Z { get; set; }

        public override SqlGeography AsSqlGeography()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginGeography(OpenGisGeographyType.Point);
            b.BeginFigure(this.Lat, this.Long, this.Z, this.Time.HasValue ? this.Time.Value.ToOADate() : (double?)null);
            b.EndFigure();
            b.EndGeography();
            return b.ConstructedGeography;
        }
    }

    [DataContract(Namespace = "")]
    public class RouteView : GeographyView
    {
        [DataMember]
        public List<MapCoordinate> Points { get; set; }

        [DataMember(Name = "Len")]
        public double Length { get; set; }

        public RouteView()
            : base()
        {
            this.Points = new List<MapCoordinate>();
        }

        public override SqlGeography AsSqlGeography()
        {
            SqlGeographyBuilder b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginGeography(OpenGisGeographyType.LineString);
            if (this.Points.Count > 0)
            {
                b.BeginFigure(this.Points[0].Lat, this.Points[0].Long, this.Points[0].Z, this.Points[0].Time.HasValue ? (double?)null : this.Points[0].Time.Value.ToOADate());
            }
            for (int i = 1; i < this.Points.Count; i++)
            {
                b.AddLine(this.Points[i].Lat, this.Points[i].Long, this.Points[i].Z, this.Points[i].Time.HasValue ? (double?)null : this.Points[i].Time.Value.ToOADate());
            }
            b.EndFigure();
            b.EndGeography();
            return b.ConstructedGeography;
        }
    }

    [DataContract]
    public class MapCoordinate
    {
        [DataMember]
        public double Lat { get; set; }

        [DataMember]
        public double Long { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double? Z { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? Time { get; set; }

    }

    [DataContract(Namespace="")]
    public class MapDataView
    {
        List<GeographyView> _items = new List<GeographyView>();

        [DataMember]
        public List<GeographyView> Items { get { return this._items; } }

        [DataMember(EmitDefaultValue=false)]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string[] Messages { get; set; }
    }
}
