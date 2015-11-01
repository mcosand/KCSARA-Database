<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<Kcsar.Database.Model.Training>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<% Training t = ViewData.Model; %>

<div>
<div class="MInfo""><%= this.ModelData(t) %></div>
<h2><%= t.Title %></h2>
<table>
<% if (t.HostUnitId.HasValue) { %> <tr><th>Host Unit:</th><td><%: t.HostUnit.DisplayName %></td></tr> <% } %>
<tr><th>Courses:</th><td>
<% foreach (TrainingCourse c in t.OfferedCourses)
   { %>
<%= c.DisplayName%><br />
<% } %>
</td></tr>
</table>
<%= Html.PopupActionLink<TrainingController>(x => x.Edit(t.Id), "Edit") %>
</div>