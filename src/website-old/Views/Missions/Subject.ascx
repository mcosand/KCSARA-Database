<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Kcsar.Database.Model.Subject>" %>
<%@ Import Namespace="Kcsara.Database.Web" %>
<div style="position: relative">
  <div style="font-weight: bold; font-size: 1.1em;">
    <%= Html.Encode(ViewData["prefix"])+Html.Encode(Model.LastName) %>,
    <%= Html.Encode(Model.FirstName) %>
  </div>
  <%= this.ModelData(Model) %>
  <p>
    Gender:
    <%= Html.Encode(Model.Gender) %>
  </p>
  <p>
    BirthYear:
    <%= Html.Encode(Model.BirthYear) %>
  </p>
  <p>
    Address:
    <%= Html.Encode(Model.Address) %>
  </p>
  <p>
    HomePhone:
    <%= Html.Encode(Model.HomePhone) %>
  </p>
  <p>
    WorkPhone:
    <%= Html.Encode(Model.WorkPhone) %>
  </p>
  <p>
    OtherPhone:
    <%= Html.Encode(Model.OtherPhone) %>
  </p>
  <p>
    Comments:
    <%= Html.Encode(Model.Comments) %>
  </p>
</div>
