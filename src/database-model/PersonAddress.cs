namespace Kcsar.Database.Model
{
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Data.SqlTypes;
  using Microsoft.SqlServer.Types;

  public class PersonAddress : ModelObject, IAddressGeography
  {
    [Required]
    public string Street { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string State { get; set; }
    [Required]
    public string Zip { get; set; }
    public string Geo { get; set; }
    public int Quality { get; set; }
    public int RosterVisibility { get; set; }
    public virtual Member Person { get; set; }

    public string SimpleText { get { return string.Format("{0}, {1}, {2} {3}", this.Street, this.City, this.State, this.Zip); } }

    public PersonAddressType Type { get; set; }

    private SqlGeography geog = null;
    [NotMapped]
    public SqlGeography Location
    {
      get
      {
        if (geog == null)
        {
          geog = this.Geo == null ? null : SqlGeography.STGeomFromText(new SqlChars(this.Geo.ToCharArray()), 4326);
        }
        return geog;
      }
      set
      {
        geog = value;
        this.Geo = (geog == null) ? null : geog.ToString();
      }
    }


    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> {1} address: [{2}] [{3}], [{4}] [{5}]", this.Person.FullName, this.Type, this.Street, this.City, this.State, this.Zip);
    }

    public override string ToString()
    {
      return string.Format("{0} {1}: {2}, {3}, {4}", this.Person.FullName, this.Type, this.Street, this.City, this.Zip);
    }
  }
}
