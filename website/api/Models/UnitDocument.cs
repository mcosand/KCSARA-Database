
namespace Kcsara.Database.Web.api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using Model = Kcsar.Database.Model;

    public class UnitDocument
    {
        public Guid Id { get; set; }
        public NameIdPair Unit { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string SubmitTo { get; set; }
        public bool Required { get; set; }

        public int Order { get; set; }

        public int? ForMembersOlder { get; set; }
        public int? ForMembersYounger { get; set; }

        public bool CanEdit { get; set; }


        public static Expression<Func<Model.UnitDocument, UnitDocument>> UnitDocumentConversion = f => new UnitDocument
        {
            Id = f.Id,
            Unit = new NameIdPair { Name = f.Unit.DisplayName, Id = f.Unit.Id },
            Title = f.Title,
            Url = f.Url,
            SubmitTo = f.SubmitTo,
            Required = f.Required,
            ForMembersOlder = f.ForMembersOlder,
            ForMembersYounger = f.ForMembersYounger,
        };

        public void UpdateModel(Model.UnitDocument model)
        {
            if (model.Title != this.Title) model.Title = this.Title;
            if (model.Required != this.Required) model.Required = this.Required;
            if (model.SubmitTo != this.SubmitTo) model.SubmitTo = this.SubmitTo;
            if (model.Url != this.Url) model.Url = this.Url;
            if (model.Order != this.Order) model.Order = this.Order;
            if (model.ForMembersYounger != this.ForMembersYounger) model.ForMembersYounger = this.ForMembersYounger;
            if (model.ForMembersOlder != this.ForMembersOlder) model.ForMembersOlder = this.ForMembersOlder;
        }
    }
}