/*
 * Copyright 2008-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.ComponentModel.DataAnnotations;

  public interface IModelObjectRow : IValidatableObject
  {
    DateTime LastChanged { get; set; }
    string ChangedBy { get; set; }
    Guid Id { get; }
    string GetReportHtml();
  }
}
