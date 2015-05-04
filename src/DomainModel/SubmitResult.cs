/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsara.Database.Model
{
  using System.Collections.Generic;

  public class SubmitError
  {
    public SubmitError(string message) : this(null, message) { }
    public SubmitError(string property, string text)
    {
      this.Property = property;
      this.Text = text;
    }
    public string Property { get; set; }
    public string Text { get; set; }
  }

  public class SubmitResult
  {
    public SubmitResult()
    {
      this.Errors = new List<SubmitError>();
    }
    public List<SubmitError> Errors { get; set; }
  }

  public class SubmitResult<T> : SubmitResult
  {
    public SubmitResult() : base() { }
    public T Data { get; set; }
  }
}