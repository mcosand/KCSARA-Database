/*
 * Copyright 2010-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data.Events
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Data.SqlTypes;
  using Microsoft.SqlServer.Types;

  [Table("EventGeographies")]
  public class EventGeographyRow : ModelObjectRow
  {
    public Guid? InstanceId { get; set; }
    public string Kind { get; set; }
    public DateTime? Time { get; set; }
    public string Description { get; set; }
    public string LocationBinary { get; set; }
    public string LocationText { get; set; }
    [ForeignKey("EventId")]
    public virtual SarEventRow Event { get; set; }
    public Guid EventId { get; set; }

    private SqlGeography geog = null;
    [NotMapped]
    public SqlGeography Geography
    {
      get
      {
        if (geog == null)
        {
          geog = this.LocationBinary == null ? null : SqlGeography.STGeomFromText(new SqlChars(this.LocationBinary.ToCharArray()), 4326);
        }
        return geog;
      }
      set
      {
        geog = value;
        this.LocationBinary = geog.ToString();
      }
    }

    #region IModelObject Members

    public override string GetReportHtml()
    {
      return "report";
    }

    #endregion
  }
}
