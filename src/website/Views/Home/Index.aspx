<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Strings.DatabaseName %></h2>
    <p>
        Welcome to the database tracking the activities of the <%: Strings.DatabaseName %>.
        If you don't see a menu to the left, please <%= Html.ActionLink<HomeController>(f => f.Login(null), "login") %>.
    </p>
<%--    <p>View a map of our <%= Html.ActionLink<MapController>(f => f.Index(null), "mission activity") %>.</p>--%>
</asp:Content>
