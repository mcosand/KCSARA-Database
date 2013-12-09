<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<IEnumerable<GroupView>>" %>
<%@ Import Namespace="Kcsar.Membership" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
  bool isAdmin = (bool)ViewData["IsAdmin"];
  Guid userId = (Guid)ViewData["UserId"];
   %>
<%= isAdmin ? Html.ActionLink<AdminController>(x => x.CreateGroup(), "Create Group").ToString() : "" %>
<table id="t" class="data-table zebra-sorted">
<thead>
<tr><th></th><th>Group Name</th><th>Email</th><th>Owners</th><th>External</th><th></th></tr>
</thead>
<tbody>
<% foreach (GroupView group in ViewData.Model)
   {
     bool canEdit = (isAdmin || group.Owners.Where(f => f.Id == userId).Count() > 0);
 %>
<tr>
  <td><% if (isAdmin) { %>
  <%= Html.ActionLink<AdminController>(x => x.DeleteGroup(group.Name), "X") %>
  <%= Html.ActionLink<AdminController>(x => x.EditGroup(group.Name), "Edit") %>
  <% } %></td>
  <td><%: group.Name %></td>
  <td><%: group.EmailAddress %></td>
  <td><%: string.Join(", ", group.Owners.Select(f => f.Name).ToArray()) %></td>
  <td><%: string.Join(", ", group.Destinations) %></td>
  <td><%: Html.ActionLink<AdminController>(x => x.GroupMembership(group.Name), (canEdit ? "Edit" : "View") + " Members") %></td>
</tr>
<% } %>
</tbody>
<tfoot>
<tr><th colspan="6" style="text-align:left;"><%= ViewData.Model.Count() %> Groups</th></tr>
</tfoot>
</table>
</asp:Content>
