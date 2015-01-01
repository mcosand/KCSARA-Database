<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

  <h2>Index</h2>
  <ul>
    <li><%= Html.ActionLink<AdminController>(f => f.Groups(), "Groups") %></li>
    <% if (Page.User.IsInRole("cdb.admins"))
       { %>
    <li><%= Html.ActionLink<AdminController>(f => f.Accounts(), "Account Management") %></li>
    <li style="margin-top: 1em;"><%= Html.ActionLink<LogController>(f => f.Index(), "Log") %></li>
    <li style="margin-top: 1em;"><%= Html.ActionLink<AccountController>(f => f.InactiveMemberAccounts(), "Inactive Member Accounts") %></li>
    <li style="margin-top: 1em;"><%= Html.ActionLink<AdminController>(f => f.Sql(), "Execute database script") %></li>
    <li style="margin-top: 1em;"><%= Html.ActionLink<AdminController>(f => f.DisconnectedPhotos(), "View Disconnected Photos") %></li>
    <% } %>
  </ul>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
