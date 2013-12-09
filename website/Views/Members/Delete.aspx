<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<Member>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<p>Are you sure you want to remove member <%= ViewData.Model.FullName %>?</p>
<% using (Html.BeginForm<MembersController>(x => x.Delete(ViewData.Model.Id)))
   { %>
  <input type="submit" value="<%= Strings.ConfirmDelete %>" />
<% } %>
</asp:Content>
