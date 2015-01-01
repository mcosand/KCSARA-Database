<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Mobile.Master" Inherits="System.Web.Mvc.ViewPage<ExpandedRowsContext>" %>

<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        <%: Model.SarEvent.StateNumber %>
        <%: Model.SarEvent.Title %></h2>
    <table id="roster" cellpadding="0">
        <thead>
            <tr><th></th><th>Name</th><th>Unit</th><th>Role</th></tr>
        </thead>
        <tbody>
            <%
                IEnumerable<IRosterEntry> rows = Model.Rows.Cast<MissionRoster>()
                        .OrderBy(x => x.Unit.DisplayName + ":" + x.Person.ReverseName + ":" + x.TimeIn.Value.ToString("yyyy-MM-dd HH:mm")).Cast<IRosterEntry>();

                foreach (IRosterEntry row in rows)
                {
                    IRosterEvent evt = row.GetEvent();
                    MissionRoster mrow = row as MissionRoster;
            %>
            <tr><td><div class="rstr rstr<%: mrow.TimeOut.HasValue ? "" : mrow.InternalRole %>"></div></td><td><%: mrow.Person.FullName %></td><td><%: mrow.Unit.DisplayName %></td><td><%: mrow.TimeOut.HasValue ? "" : mrow.InternalRole %></td></tr>
            <% } %>
        </tbody>
    </table>
    <script type="text/javascript">
        $(document).ready(function () {
            $("#roster").tablesorter({ widgets: ['zebra'], headers: { 0: { sorter: 'link'}} });
        }
);
</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
<style>
    .rstr { width:1em; height:1em; }
    .rstrField { background-color:#008800; }
    .rstrBase { background-color:Orange; }
    .rstrOL { background-color:Red; }
    .rstrResponder { background-color:Yellow; }
    .rstrInTown { background-color:Gray; }
</style>
</asp:Content>
