<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<Kcsar.Database.Model.Mission_Old>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<% Mission_Old m = ViewData.Model; %>

<div>
<div class="MInfo"><%= this.ModelData(m) %></div>
<h2><%= m.Title %></h2>
<table>
<tr><th>DEM #</th><td><%= m.StateNumber %></td></tr>
<tr><th>Dates</th><td><%= m.StartTime.ToString("yyyy-MM-dd HH:mm") %> - <%= string.Format("{0:yyyy-MM-dd HH:mm}", m.StopTime) %></td></tr>
<tr><th>County</th><td><%= m.County %></td></tr>
<tr><th>Type</th><td><%= m.MissionType %></td></tr>
</table>
<% if (ViewData["CanEdit"] != null && (bool)ViewData["CanEdit"]) { %>
<%= Html.PopupActionLink<MissionsController>(x => x.Edit(m.Id), "Edit", 450, 540) %>
<% } %>
</div>