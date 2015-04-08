/*
 * Copyright 2013-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.api.Models
{
  using System;
  using System.Linq.Expressions;
  using Kcsar.Database.Data;
  using Model = Kcsar.Database.Model;

  public class MemberUnitDocument
  {
    public Guid DocId { get; set; }
    public Guid? Id { get; set; }
    public NameIdPair Unit { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string SubmitTo { get; set; }
    public bool Required { get; set; }

    internal Model.DocumentStatus StatusEnum { get; set; }
    public string Status
    {
      get { return StatusEnum.ToString(); }
      set { this.StatusEnum = (Model.DocumentStatus)Enum.Parse(typeof(Model.DocumentStatus), value, true); }
    }

    public DateTime? StatusDate { get; set; }

    public MemberUnitDocumentActions Actions { get; set; }

    public static Expression<Func<UnitDocumentRow, MemberUnitDocument>> UnitDocumentConversion = f => new MemberUnitDocument
    {
      DocId = f.Id,
      Unit = new NameIdPair { Name = f.Unit.DisplayName, Id = f.Unit.Id },
      Title = f.Title,
      Url = f.Url,
      Actions = ((f.SubmitTo == null) ? MemberUnitDocumentActions.UserReview : MemberUnitDocumentActions.Submit) | MemberUnitDocumentActions.Complete,
      StatusEnum = Model.DocumentStatus.NotStarted,
      SubmitTo = f.SubmitTo,
      Required = f.Required,
    };

    public static Expression<Func<MemberUnitDocumentRow, MemberUnitDocument>> MemberUnitDocumentConversion = f => new MemberUnitDocument
    {
      DocId = f.Document.Id,
      Id = f.Id,
      Unit = new NameIdPair { Name = f.Document.Unit.DisplayName, Id = f.Document.Unit.Id },
      Title = f.Document.Title,
      Url = f.Document.Url,
      SubmitTo = f.Document.SubmitTo,
      Actions = ((f.Document.SubmitTo == null && f.Status < Model.DocumentStatus.Complete) ? MemberUnitDocumentActions.UserReview : MemberUnitDocumentActions.None)
          | ((f.Document.SubmitTo != null && f.Status < Model.DocumentStatus.Submitted) ? MemberUnitDocumentActions.Submit : MemberUnitDocumentActions.None)
          | ((f.Status != Model.DocumentStatus.Complete && f.Status != Model.DocumentStatus.NotApplicable) ? MemberUnitDocumentActions.Complete : MemberUnitDocumentActions.None)
          | ((f.Status == Model.DocumentStatus.Submitted || f.Status == Model.DocumentStatus.Complete) ? MemberUnitDocumentActions.Reject : MemberUnitDocumentActions.None),
      StatusEnum = f.Status,
      StatusDate = (f.UnitAction == null) ?
                  (f.MemberAction == null ? null : f.MemberAction) :
                  (f.MemberAction == null ? f.UnitAction : (f.UnitAction > f.MemberAction ? f.UnitAction : f.MemberAction)),
      Required = f.Document.Required,
    };
  }

  [Flags]
  public enum MemberUnitDocumentActions
  {
    None = 0,
    UserReview = 1,
    Submit = 2,
    Complete = 4,
    Reject = 8,
    All = 0xff
  }
}
