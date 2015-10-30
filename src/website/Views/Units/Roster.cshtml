<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<IEnumerable<UnitMembership>>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsar.Database.Model" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  <h2><%= ((SarUnit)ViewData["Unit"]).DisplayName %> Roster</h2>
  
  <table id="t">
    <thead>
      <tr><th>Name</th><th>DEM #</th><th>Status</th><th>Since</th></tr>
    </thead>
    <tbody>
      <% foreach (UnitMembership um in ViewData.Model)
         {%>
        <tr>
          <td><%= Html.ActionLink<MembersController>(x => x.Detail(um.Person.Id), um.Person.ReverseName) %></td>
          <td><%= um.Person.DEM %></td>
          <td><%= um.Status.StatusName %></td>
          <td><%= string.Format("{0:yyyy-MM-dd}", um.Activated) %></td>
          </tr>
      <% } %>
    </tbody>
  </table>
  <script>
  $(document).ready(function() {
    $("#t").tablesorter({ widgets: ['zebra'] });
  });
</script>
</asp:Content>
  