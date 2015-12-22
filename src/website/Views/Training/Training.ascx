<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<Kcsar.Database.Model.Training_Old>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<% Training_Old t = ViewData.Model; %>

<div>
<div class="MInfo""><%= this.ModelData(t) %></div>
<h2><%= t.Title %></h2>
<table>
<% if (t.HostUnits.Count > 0)
  { %>
  <tr><th>Host Units:</th><td>
    <% foreach (SarUnit u in t.HostUnits.OrderBy(f => f.DisplayName))
      { %>
    <%: u.DisplayName %><br />
    <%} %>
    </td></tr>
  <% } %>
<tr><th>Courses:</th><td>
<% foreach (TrainingCourse c in t.OfferedCourses)
  { %>
<%= c.DisplayName%><br />
<% } %>
</td></tr>
</table>
<%= Html.PopupActionLink<TrainingController>(x => x.Edit(t.Id), "Edit") %>
</div>