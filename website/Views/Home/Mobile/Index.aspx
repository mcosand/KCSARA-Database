<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Mobile.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
<ul id="list" class="mobileSelect">
<li><%: Html.ActionLink<MissionsController>(f => f.Index(), "Active Missions") %></li>
<li><%: Html.ActionLink<MembersController>(f => f.Index(), "KCSARA Members") %></li>
</ul>
</asp:Content>
