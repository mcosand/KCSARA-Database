/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models
{
  using System;

  public class Person
  {
    public Guid? Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
  }
}