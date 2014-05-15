<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<IEnumerable<MemberSummary>>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.api.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <% if (Page.User.IsInRole("cdb.admins") && ViewData["filtered"] == null) { %><div><%=
        Html.PopupActionLink<MembersController>(f => f.Create(), Strings.ActionNew)%></div> <% } %>
  <% if (ViewData["filtered"] != null) { %><div>Filtered List. View the unfiltered list to add a new person.</div><% } %>
  <table id="the-table" cellpadding="0" class="data-table">
    <thead>
      <tr>
        <th>DEM</th>
        <th>
          Name
        </th>
        <th>Active Units</th>
      </tr>
    </thead>
    <tbody>
      <%
          foreach (MemberSummary m in ViewData.Model)
         {%>
        <tr>
          <td><%= m.WorkerNumber %></td>
          <td><%= Html.ActionLink<MembersController>(x => x.Detail(m.Id), m.Name) %></td>
          <td><%= string.Join(", ", m.Units.Values.OrderBy(f => f).ToArray()) %></td>
         </tr>
      <% } %>
    </tbody>
  </table>

<script type="text/javascript">
  $(document).ready(function() {
    $("#the-table").tablesorter({ widgets: ['zebra'], headers: { 2: { sorter: 'link'}} });
  });

</script>
</asp:Content>
