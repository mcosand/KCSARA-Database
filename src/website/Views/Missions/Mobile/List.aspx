<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Mobile.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<EventSummaryView>>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2>Last 10 Missions</h2>
<ul class="mobileSelect">
<% foreach (EventSummaryView m in ViewData.Model.Take(10))
{
%>
<li><%: Html.ActionLink<MissionsController>(f => f.Roster(m.Id), m.Number + " " + m.Title) %></li>
<% } %>
</ul>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
