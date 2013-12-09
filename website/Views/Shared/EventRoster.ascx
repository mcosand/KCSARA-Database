<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<ExpandedRowsContext>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>

<% Html.RenderPartial(Model.Type.ToString(), Model.SarEvent); %>

<h2><%= Model.Type %> Roster</h2>
<% if ((bool)(ViewData["CanEditRoster"] ?? false)) { %>
<%= Html.ActionLink("Edit Roster", "EditRoster", ViewData["TargetController"].ToString(), new { id = Model.SarEvent.Id }, null) %>
<% } %>
<% if (!(bool)(ViewData["IsTemp"] ?? false)) {
       Response.Write(Html.ActionLink("ICS-211", "ICS211", new { id = Model.SarEvent.Id }, null));
       if (Model.Type == RosterType.Mission) Response.Write(" " + Html.ActionLink("Emails", "ResponderEmails", new { id = Model.SarEvent.Id }, null));
   } %>
<table id="roster" cellpadding="0" class="data-table">
  <thead>
    <tr>
      <th rowspan="2">Name</th>
      <th rowspan="2">DEM</th>
      <% Html.RenderPartial("RosterAddedHeader", Model); %>
      <% for (int i = 0; i < Model.NumDays; i++) { %>
        <th colspan="2">
          <%= Model.RosterStart.AddDays(i).ToShortDateString()%>
        </th>
      <% } %>
      <th rowspan="2">Hours</th>
      <th rowspan="2">Miles</th>
    </tr>
    <tr>
      <% for (int i = 0; i < Model.NumDays; i++) { %>
        <th style="font-size:85%">Time In</th>
        <th style="font-size:85%">Time Out</th>
      <% } %>
    </tr>
  </thead>
  <tbody>
    <%
    IEnumerable<IRosterEntry> rows = (Model.Type == RosterType.Mission)
        ? Model.Rows.Cast<MissionRoster>()
            .OrderBy(x => x.Unit.DisplayName + ":" + x.Person.ReverseName + ":" + x.TimeIn.Value.ToString("yyyy-MM-dd HH:mm")).Cast<IRosterEntry>()
        : Model.Rows
            .OrderBy(x => x.Person.ReverseName + ":" + x.TimeIn.Value.ToString("yyyy-MM-dd HH:mm"));
    
    foreach (IRosterEntry row in rows)
    {      
      IRosterEvent evt = row.GetEvent();
      MissionRoster mrow = row as MissionRoster;
      TrainingRoster trow = row as TrainingRoster;
      %>
    <tr>
        <td><div class="MInfo"><%= this.ModelData(row) %></div>
        <%= Html.ActionLink<MembersController>(x => x.Detail(row.Person.Id), row.Person.ReverseName)%>
        <% if (Model.Type == RosterType.Mission && ((MissionRoster)row).Animals.Count > 0)
           { %>
           <div style="padding-left:1em;">
           <% foreach (Animal a in ((MissionRoster)row).Animals.Select(f => f.Animal)) { %>
            <div><%= string.Format("with {0}", a.Name) %></div>
           <% } %>
           </div>
        <% } %>
        </td>
        <td><%= row.Person.DEM %></td>
        <% if (mrow != null) {
        %>
          <td><%= (mrow.Unit == null) ? "" : mrow.Unit.DisplayName %></td>
          <td><%: mrow.InternalRole %></td>
        <% } else if (trow != null) {%>
             <td><%= string.Join("<br/>", ((TrainingRoster)row).TrainingAwards.Select(f => f.Course.DisplayName + " <span class=\"MInfo\">" +this.ModelData(f) + "</span>").ToArray()) %></td>
        <% } else { %>
          <td></td>
        <% } %>

        <% for (int i = 0; i < Model.NumDays; i++) { %>
          <td><%= (row.TimeIn.HasValue && (row.TimeIn.Value.Date == Model.RosterStart.AddDays(i))) ? row.TimeIn.Value.ToString("HHmm") : ""%></td>
          <td><%= (row.TimeOut.HasValue && (row.TimeOut.Value.Date == Model.RosterStart.AddDays(i))) ? row.TimeOut.Value.ToString("HHmm") : ""%></td>
        <% } %>

        <td class="r"><%= ((row.TimeIn.HasValue && row.TimeOut.HasValue) ? (row.TimeOut.Value - row.TimeIn.Value).TotalHours.ToString("0.0") : "") 
                    + ((mrow != null && mrow.OvertimeHours.HasValue) ? string.Format(" [{0:0.0}]", mrow.OvertimeHours.Value) : "") %></td>
        <td class="r"><%= row.Miles %></td>
      </tr>
  <% }%>
  </tbody>
  <tfoot>
  <tr><th colspan="<%= Model.NumDays*2 + ((Model.Type == RosterType.Mission) ? 4 : 3) %>"><%: rows.Select(f => f.Person.Id).Distinct().Count() %> Persons</th>
  <th class="r"><%: rows.Sum(f => f.Hours).Value.ToString("0.0") %></th><th class="r"><%: rows.Sum(f => f.Miles) %></th>
  </tr>
  </tfoot>
</table>

<% if ((bool)(ViewData["CanEditRoster"] ?? false)) Response.Write(Html.ActionLink("Edit Roster", "EditRoster", (Model.Type == RosterType.Mission) ? "Missions" : "Training", new { id = Model.SarEvent.Id }, null)); %>
<script type="text/javascript">
    $(document).ready(function () {
        $("#roster").tablesorter({ widgets: ['zebra'], headers: { 0: { sorter: 'link'}} });
        $('#showMInfo').removeAttr('checked').click(function () {
            $('.MInfo').css("display", this.checked ? "inline" : "none");
        });
        $('#showMInfoP').show();
    }
);
</script>

<% if (ViewData.Model.BadRows.Count() > 0) { %>
<h3>Bad Entries:</h3>
<% foreach (IRosterEntry badRow in ViewData.Model.BadRows) { %>
<table>
<tr><th>Name</th><td><%= badRow.Person.ReverseName%></td></tr>
<% if (badRow is MissionRoster)
   { %>
  <tr><th>Unit</th><td><%= (((MissionRoster)badRow).Unit == null) ? "" : ((MissionRoster)badRow).Unit.DisplayName%></td></tr>
<% } %>
<tr><th>Time In</th><td><%= badRow.TimeIn.HasValue ? badRow.TimeIn.Value.ToString("MM/dd/yy HH:mm") : "unknown" %></td></tr>
<tr><th>Time Out</th><td><%= badRow.TimeOut.HasValue ? badRow.TimeOut.Value.ToString("MM/dd/yy HH:mm") : "unknown" %></td></tr>
<tr><th>Miles</th><td><%= badRow.Miles%></td></tr>
</table>
<% }
} %>

