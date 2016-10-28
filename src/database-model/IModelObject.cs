namespace Kcsar.Database.Model
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using Sar.Database.Data;

  public interface IModelObject : IRowWithId, IValidatableObject
  {
    DateTime LastChanged { get; set; }
    string ChangedBy { get; set; }
    string GetReportHtml();
  }
}
