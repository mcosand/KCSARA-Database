<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<RosterRowsContext>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<%
  IEnumerable<IRosterEntry> roster = ViewData.Model.Rows;
  RosterType t = ViewData.Model.Type;
 %>

    <table id="rosterevents<%= t %>" border="0" cellpadding="0" class="data-table">
    <thead>
  <tr>
    <th>DEM</th>
    <th>Start Time</th>
    <th>Title</th>
      <% if (t == RosterType.Mission) { %>
        <th>Unit</th>
      <% } else { %>
        <td></td>
      <% } %>
  <th>Time In</th>
  <th>Time Out</th>
  <th>Hours</th>
  <th>Miles</th>
  </tr>
   </thead>
   <tbody>
  <% int count = 0;
    foreach (IRosterEntry row in roster.OrderByDescending(x => x.TimeIn)) {
       IRosterEvent evt = row.GetEvent();      
  %>
  
    <tr class="<%= ((count++ % 2) != 0) ? "row-alternating" : "" %>">
      <td><%= evt.StateNumber %></td>
      <td><%= evt.StartTime.ToString("MM-dd-yy") %></td>
      <td><%= Html.ActionLink(
            row.GetEvent().Title,
            "Roster",
            (ViewData.Model.Type == RosterType.Mission) ? "Missions" : "Training",
            new { id = evt.Id },
            null) %>
      </td>
 
      <% if (row is MissionRoster) { %>
        <td><%= ((MissionRoster)row).Unit.DisplayName%></td>
      <% } else { %>
        <td></td>
      <% } %>
      
      <%-- Time In and Time Out. In 2400 military time. If the time is not on the same date as the event start,
      prepend the difference in days. (ex: Mission starts on Sunday, I sign in that night, and out the next day.
      The times would be printed "2300" "1+0900" --%>
      <td class="r"><%=
            ((row.TimeIn.Value.Date != evt.StartTime.Date)
                ? ((row.TimeIn.Value.Date - evt.StartTime.Date).Days.ToString() + "+")
                : "")
            + row.TimeIn.Value.ToString("HHmm") %></td>
      <td class="r"><%=
            ((row.TimeOut.HasValue && row.TimeOut.Value.Date != evt.StartTime.Date)
                  ? (row.TimeOut.Value.Date - evt.StartTime.Date).Days.ToString() + "+"
                  : "")
            + ((row.TimeOut.HasValue)
                  ? row.TimeOut.Value.ToString("HHmm") 
                  : "")
      %></td>
      <td class="r"><%= (row.TimeOut.HasValue) ? (row.TimeOut.Value - row.TimeIn.Value).TotalHours.ToString("0.0") : "" %></td>
      <td class="r"><%= row.Miles %></td>
    </tr>
  <% } %>
  </tbody>
  <tfoot>
    <tr>
      <th class="l" colspan="6"><%= string.Format("{0} {1}", roster.GroupBy(x => x.GetEvent().Id).Count(), (ViewData.Model.Type == RosterType.Mission) ? "Missions" : "Training") %></th>
      <th class="r"><%= roster.Sum(x => x.TimeOut.HasValue ? (x.TimeOut.Value - x.TimeIn.Value).TotalHours : 0).ToString("0.0") %></th>
      <th class="r"><%= roster.Sum(x => x.Miles) %></th>
    </tr>
  </tfoot>
</table>
<script type="text/javascript">
  $(document).ready(function() {
      $("#rosterevents<%= t %>").tablesorter({ widgets: ['zebra'], headers: { 2: { sorter: 'link'}} });
  });
</script>
