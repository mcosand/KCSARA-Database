<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<Kcsar.Database.Model.AuditLog>>" %>
<table id="t" class="data-table">
<thead>
<tr><th>Time</th><th>Table</th><th></th><th>Change</th><th>User</th></tr>
</thead>
<tbody>
<% foreach (Kcsar.Database.Model.AuditLog row in Model) { %>
 <tr>
  <td><%= row.Changed.ToShortDateString() %><br /><%= row.Changed.ToShortTimeString() %></td>
  <td><%= row.Collection %></td>
  <td><img src="<%= Url.Content(string.Format("~/Content/images/{0}.png", row.Action)) %>" /></td>
  <td><%= row.Comment %></td>
  <td><%= row.User %></td>
  </tr>
<% } %>
</tbody>
<script>
  $(document).ready(function() {
    $("#t").tablesorter({ widgets: ['zebra'] });
  });
</script>
</table>