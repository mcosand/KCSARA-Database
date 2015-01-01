<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<UnitStatus>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<p>Are you sure you want to remove unit status 
<%= string.Format(
  "Unit={0}, Status={1}",
  ViewData.Model.Unit.DisplayName,
  ViewData.Model.StatusName)
  %>?</p>
<% using (Html.BeginForm())
   { %>
   <%= Html.Hidden("Id") %>
  <input type="submit" value="Yes, Delete" />
<% } %>
</asp:Content>
