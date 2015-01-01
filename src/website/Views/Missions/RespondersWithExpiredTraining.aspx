<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MissionRosterWithExpiredTrainingView[]>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsar.Database" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2>Mission Responders with Expired Training</h2>

  <table class="data-table" id="table">
  <thead><tr><th>Worker #</th><th>Member</th><th>Date</th><th>Mission #</th><th>Mission</th><th>Expired Trainings</th></tr></thead>
  <tbody>
  <% foreach (var row in Model)
     { %>
    <tr><td><%: row.Member.WorkerNumber %></td><td><%: row.Member.Name %></td><td><%: row.Mission.StartTime %></td><td><%: row.Mission.Number %></td><td><%: row.Mission.Title %></td><td><%: string.Join(", ", row.ExpiredTrainings) %></td></tr>
  <% } %>
  </tbody>
  </table>
  
<script type="text/javascript">
    $(document).ready(function () {
        $('#table').tablesorter({ widgets: ['zebra'] });
    });
</script>
</asp:Content>