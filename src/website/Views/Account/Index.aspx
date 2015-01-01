<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

  <h2>
    My Account</h2>
  <ul>
    <li>
      <%= Html.ActionLink<AccountController>(x => x.ChangePassword(), "Change Password") %></li>
      <li><%= Html.PopupActionLink<AccountController>(x => x.Settings(), "Change Settings", 500) %></li>
  </ul>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
