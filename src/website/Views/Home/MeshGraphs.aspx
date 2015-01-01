<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Kcsara.Database.Web.Model.MeshNodeStatus>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Mesh Graphs for <%= Model.Name %> <%= ViewData["type"] ?? "radios" %></h2>
    <img src="<%= Url.Content(string.Format("~/Content/auth/nodes/{0}-{1}hourly.gif", Model.Name, ViewData["type"])) %>" /><br />
    <img src="<%= Url.Content(string.Format("~/Content/auth/nodes/{0}-{1}daily.gif", Model.Name, ViewData["type"])) %>" /><br />
    <img src="<%= Url.Content(string.Format("~/Content/auth/nodes/{0}-{1}weekly.gif", Model.Name, ViewData["type"])) %>" /><br />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="secondaryNav" runat="server">
</asp:Content>
