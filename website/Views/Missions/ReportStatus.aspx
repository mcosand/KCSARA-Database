<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true"
  Inherits="System.Web.Mvc.ViewPage<IEnumerable<EventReportStatusView>>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsar.Database.Model" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <table id="list" cellpadding="0" class="data-table" style="clear:both;">
    <thead>
      <tr>
        <th>DEM</th>
        <th>Date</th>
        <th>
          Title
        </th><th>Units</th>
        <th>Persons</th><th>Still Signed In</th><th>Log Entries</th><th>Geography</th><th>Subjects</th><th>Documents</th>
      </tr>
    </thead>
    <tbody>
      <% foreach (EventReportStatusView m in ViewData.Model)
         {
         %>
        <tr>
          <td><%: m.Number %></td>
          <td><%: m.StartTime.ToShortDateString() %></td>
          <td><%: m.IsTurnaround ? "[" : "" %><%: Html.ActionLink<MissionsController>(x => x.Roster(m.Id), m.Title) %><%: m.IsTurnaround ? "]" : "" %></td>
          <td><%: string.Join(", ", m.Units) %></td>
          <td class="r"><%: m.Persons %></td>
          <td class="r"><%: Html.ActionLink<MissionsController>(x => x.Roster(m.Id), m.NotSignedOut.ToString()) %></td>
          <td class="r"><%: Html.ActionLink<MissionsController>(x => x.Log(m.Id), m.LogCount.ToString()) %></td>
          <td class="r"><%: Html.ActionLink<MissionsController>(x => x.Geography(m.Id), m.GeoCount.ToString()) %></td>
          <td class="r"><%: Html.ActionLink<MissionsController>(x => x.Subjects(m.Id), m.SubjectCount.ToString()) %></td>
          <td class="r"><%: Html.ActionLink<MissionsController>(x => x.Documents(m.Id.ToString()), m.DocumentCount.ToString()) %></td>
         </tr>
      <% } %>
    </tbody>
  </table>
<script type="text/javascript">
    $(document).ready(function () {
        $("#list").tablesorter({ widgets: ['zebra'], headers: { 3: { sorter: 'link'}} });
    });
</script>
</asp:Content>
