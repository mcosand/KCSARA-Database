<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<UnitMembership>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<p>Are you sure you want to remove unit membership 
<%= string.Format(
  "Member={0}, Unit={1}, Status={2}, Activated={3:s}",
  ViewData.Model.Person.FullName,
  ViewData.Model.Unit.DisplayName,
  ViewData.Model.Status.StatusName,
  ViewData.Model.Activated)
  %>?</p>
<% using (Html.BeginForm<MembersController>(x => x.DeleteMembership(ViewData.Model.Id)))
   { %>
  <input type="submit" value="Yes, Delete" />
<% } %>
</asp:Content>
