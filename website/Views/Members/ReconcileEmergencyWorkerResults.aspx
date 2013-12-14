<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Kcsara.Database.Web.Model.ReconcileEmergencyWorkersViewModel>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Reconcile Emergency Worker Results</h2>
    <p>Rows Processed: <%: Model.RowCount  %></p>
    <p>Highlighting: Yellow highlights (except WAC Current column) indicate a difference between this database
    and KCSO's records. The value shown is KCSO's value. Please determine if the KCSARA database or KCSO records
    need to be updated and take appropriate actions to do so. Green highlights indicaate Novices that, according
    to this database, have been in the rank for more than a year but have not been moved to Field status.</p>
    <p>
    <% foreach (var row in Model.NoMatch)
       { %>
    <%: row%><br />
    <% } %>
    </p>

    <input type="checkbox" id="showNonDEM" checked="checked" /> Show KCSARA members that are not Emergency Workers.

    <% foreach (var unitData in Model.MembersByUnit.OrderBy(f => f.Key))
       { %>
    <h3><%: unitData.Key %></h3>
    <table class="data-table">
        <tr><th>DEM #</th><th>Name</th><th>WAC Rank<br />(KCSO/KCSARA)</th><th>Unit Status</th><th>WAC Current</th><th># Missions<br />(18 months)</th><th># Training<br />(18 months)</th></tr>
        <% foreach (var row in unitData.Value) { %>
        <tr<%= (row.WacLevel == "None") ? " class=\"nonDEM\"" : "" %>>
            <td<%= row.DemIsNew ? " class=\"highlight\"" : "" %>><%: row.DEM %></td>
            <td><%: Html.ActionLink<MembersController>(f => f.Detail(row.Id), row.Name) %></td>
            <td<%= row.WacIsNew ? " class=\"highlight\"" : row.IsOldNovice ? " class=\"oldNovice\"" : "" %>><%: row.WacLevel %></td>
            <td><%= row.UnitStatus %></td>
            <td<%= (row.TrainingCurrent.HasValue && !row.TrainingCurrent.Value) ? " class=\"highlight\"" : "" %>><%: 
                row.TrainingCurrent.HasValue ? row.TrainingCurrent.Value ? "Yes" : "No" : ""                                                                                               
            %></td>
            <td><%: row.MissionCount %></td>
            <td><%: row.TrainingCount %></td>
        </tr>
        <% } %>
    </table>

    <% } %>

    <script type="text/javascript">
        $(document).ready(function () {
            $('#showNonDEM').click(function data() {
                $(".nonDEM").css("display", this.checked ? "table-row" : "none");
            });
        });
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
<style type="text/css">
.highlight { background-color: #ffffaa; }
.oldNovice { background-color: #aaffaa; }
</style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="secondaryNav" runat="server">
</asp:Content>
