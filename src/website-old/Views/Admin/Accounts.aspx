<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<IList<AccountListRow>>" %>

<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  <% bool canEdit = (bool)ViewData["canedit"]; %>
  <%: canEdit ? Html.ActionLink<AdminController>(x => x.CreateUser(), "Create User") : MvcHtmlString.Empty %>
  <table id="t" class="data-table">
    <thead>
      <tr>
        <th></th>
        <th>Username</th>
        <th>Name</th>
        <th>Last Active</th>
        <th>Email</th>
        <th>Locked</th>
      </tr>
    </thead>
    <tbody>
      <% foreach (AccountListRow row in ViewData.Model)
        { %>
      <tr>
        <td><%: canEdit ? Html.ActionLink<AdminController>(x => x.DeleteUser(row.Username), "X") : MvcHtmlString.Empty %>
          <%: canEdit ? Html.ActionLink<AdminController>(x => x.EditUser(row.Username), "Edit") : MvcHtmlString.Empty %></td>
        <td class="a">
          <%: canEdit ? Html.ActionLink<AccountController>(x => x.Detail(row.Username), row.Username) : new MvcHtmlString(row.Username) %></td>
        <td><%: row.LastName %>, <%: row.FirstName %></td>
        <td><%: row.LastActive %></td>
        <td><%: row.Email %></td>
        <td><%: (row.IsLocked.HasValue && row.IsLocked.Value && canEdit) ? Html.ActionLink<AdminController>(f => f.UnlockAccount(row.Username), "Unlock"): MvcHtmlString.Empty %></td>
      </tr>
      <% } %>
    </tbody>
    <tfoot>
      <tr>
        <th colspan="8"><%= ViewData.Model.Count %> Users</th>
      </tr>
    </tfoot>
  </table>
  <script>
  $(document).ready(function () {
    $("#t").tablesorter({ widgets: ['zebra'] });
  });
  </script>
</asp:Content>
