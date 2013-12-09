<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Missions/Missions.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<SubjectGroup>>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <h2>
    Subjects</h2>
  <% foreach (SubjectGroup group in ViewData.Model.OrderBy(f => f.Number))  {
  %>
  <fieldset>
    <div id="g<%= group.Id %>">
      <% Html.RenderPartial("SubjectGroup", group); %>
    </div>
    <%= Html.PopupActionLink<MissionsController>(f => f.CreateSubject(group.Id, (Guid?)null), "Add Subject") %>
    <div id="b<%= group.Id %>" style="clear: both">
<div style="position:relative;">
<%= this.ModelData(group) %>
      <p>Subject Realized Lost: <%= Html.Encode(String.Format("{0:g}", group.WhenLost)) %></p>
      <p>Subject Reported Missing: <%= Html.Encode(String.Format("{0:g}", group.WhenReported)) %></p>
      <p>Call-Out Initiated: <%= Html.Encode(String.Format("{0:g}", group.WhenCalled)) %></p>
      <p>Resources' Arrival at PLS: <%= Html.Encode(String.Format("{0:g}", group.WhenAtPls)) %></p>
      <p>Subject Found: <%= Html.Encode(String.Format("{0:g}", group.WhenFound)) %></p>
      <p>PLS Latitude: <%= Html.Encode(group.PlsNorthing) %></p>
      <p>PLS Longitude: <%= Html.Encode(group.PlsEasting) %></p>
      <p>PLS Common Name: <%= Html.Encode(group.PlsCommonName) %></p>
      <p>Found Latitude: <%= Html.Encode(group.FoundNorthing) %></p>
      <p>Found Longitude: <%= Html.Encode(group.FoundEasting) %></p>
      <p>Category: <%= Html.Encode((group.Category ?? "").Replace("|", ", ")) %></p>
      <p>Cause: <%= Html.Encode((group.Cause ?? "").Replace("|", ", ")) %></p>
      <p>Behavior: <%= Html.Encode((group.Behavior ?? "").Replace("|", ", ")) %></p>
      <p>Found By: <%= Html.Encode((group.FoundTactics ?? "").Replace("|", ", ")) %></p>
      <p>Condition: <%= Html.Encode((group.FoundCondition ?? "").Replace("|", ", ")) %></p>
      <p>Comments: <%= Html.Encode(group.Comments) %></p>
    </div>
    
    <% if (Page.User.IsInRole("cdb.missioneditors"))
              Response.Write(Html.PopupActionLink<MissionsController>(f => f.EditSubjectGroup(group.Id), "Edit...", 1000, 700)); %>
    </div>
  </fieldset>
  <%       } %>
  <% if (Page.User.IsInRole("cdb.missioneditors")) Response.Write(Html.PopupActionLink<MissionsController>(f => f.CreateSubject(null, ((Guid)ViewData["missionId"])), "New subject in new group...", 500)); %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
