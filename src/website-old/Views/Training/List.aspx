<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Training_Old>>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <% if (Page.User.IsInRole("cdb.trainingeditors")) { %><div><%= Html.ActionLink<TrainingController>(c => c.Create(), "New...", 300) %></div> <% } %>
  <table id="the-table">
    <thead>
      <tr>
        <% if (Page.User.IsInRole("cdb.trainingeditors")) Response.Write("<th></th>"); %>
        <th>DEM</th>
        <th>Date</th>
        <th>
          Name
        </th>
      </tr>
    </thead>
    <tbody>
      <% foreach (Training m in ViewData.Model)
         {
         %>
        <tr>
        <% if (Page.User.IsInRole("cdb.trainingeditors"))
           { %>
          <td style="font-size:80%">
            <%= Html.PopupActionLink<TrainingController>(x => x.Edit(m.Id), "Edit", 400) %>
            <%= Html.PopupActionLink<TrainingController>(x => x.Delete(m.Id), "Delete", 200) %>
          </td>
          <% } %>
          <td><%= m.StateNumber %></td>
          <td><%= m.StartTime.ToShortDateString() %></td>
          <td><%= Html.ActionLink<TrainingController>(x => x.Roster(m.Id), m.Title) %></td>
         </tr>
      <% } %>
    </tbody>
  </table>
</asp:Content>
