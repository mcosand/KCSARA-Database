<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true"
  Inherits="System.Web.Mvc.ViewPage<Subject>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
  <h2>Subject Information</h2>
  <div class="message">
    <%= TempData["message"] %></div>
  <% using (Html.BeginForm())
     { %>
  <%=
    Html.Hidden("NewSubjectGuid")%>
  <%= Html.Hidden("MissionId") %>
  <%= Html.Hidden("GroupId") %>
<%--  <%= Html.Hidden("Mission", Model.Mission.Id) %> --%>
  
<div style="position: relative">
  <p>
    LastName:
    <%= Html.EditorFor(f => f.LastName, new { @class = "input-box" })%>
  </p>
  <p>
    FirstName:
    <%= Html.EditorFor(f => f.FirstName, new { @class = "input-box" })%>
  </p>
  <p>
    Gender:
    <%= Html.DropDownList("Gender").ToString() + Html.ValidationMessage("Gender") %>
  </p>
  <p>
    BirthYear:
    <%= Html.EditorFor(f => f.BirthYear, new { @class = "input-box" })%>
  </p>
  <p>
    Address:
    <%= Html.EditorFor(f => f.Address, new { @class = "input-box" })%>
  </p>
  <p>
    HomePhone:
    <%= Html.EditorFor(f => f.HomePhone, new { @class = "input-box" })%>
  </p>
  <p>
    WorkPhone:
    <%= Html.EditorFor(f => f.WorkPhone, new { @class = "input-box" })%>
  </p>
  <p>
    OtherPhone:
    <%= Html.EditorFor(f => f.OtherPhone, new { @class = "input-box" })%>
  </p>
  <p>
    Comments:
    <%= Html.EditorFor(f => f.Comments, new { @class = "input-box", rows = 6, cols = 20 })%>
  </p>
  <input type="submit" value="Save" />
<% } %>
</asp:Content>
