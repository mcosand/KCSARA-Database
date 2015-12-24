<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<GroupMembershipView>" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Group Membership: <%= ViewData.Model.GroupName %></h2>
    <%= Html.ActionLink<AdminController>(x => x.Groups(), "Return to Groups") %>
    <% using (Html.BeginForm()) { %>
    <table>
      <tr>
        <td><div>Current Users:</div><%= Html.ListBox("CurrentUsers", ViewData.Model.CurrentUsers, new { size = "20" })%></td> 
        <% if (ViewData.Model.CanEdit)
           { %>
          <td style="padding-top:5em;"><%= Html.SubmitButton("AddUser", "<<") %><br /><%= Html.SubmitButton("RemoveUser", ">>") %></td>
          <td><div>Available Users:</div><%= Html.ListBox("OtherUsers", ViewData.Model.OtherUsers, new { size = "20" })%></td>
        <% } %>
      </tr>
      <% if (ViewData.Model.ShowGroups)
         { %>
      <tr>
        <td><div>Current Groups:</div><p>Members of these groups are also members of this group.</p><%= Html.ListBox("CurrentGroups", ViewData.Model.CurrentGroups, new { size = "20" })%></td> 
        <% if (ViewData.Model.CanEdit)
           { %>
          <td style="padding-top:5em;"><%= Html.SubmitButton("AddGroup", "<<")%><br /><%= Html.SubmitButton("RemoveGroup", ">>")%></td>
          <td><div>Available Groups:</div><%= Html.ListBox("OtherGroups", ViewData.Model.OtherGroups, new { size = "20" })%></td>
        <% } %>
      </tr>
      <% } %>
    </table>
    <% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
