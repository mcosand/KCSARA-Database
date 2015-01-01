<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<Kcsar.Database.Model.Mission>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<p>Are you sure you want to remove mission <%= ViewData.Model.ToString() %>?</p>
<% using (Html.BeginForm<MissionsController>(x => x.Delete(ViewData.Model.Id)))
   { %>
  <input type="submit" value="Yes, Delete" />
<% } %>
</asp:Content>
