<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Member>>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Members Eligible for <%= Html.Encode(((TrainingCourse)ViewData["Course"]).DisplayName) %></h2>

<table id="list" cellpadding="0" class="data-table">
  <thead>
  <tr><th>Last Name</th><th>First Name</th></tr>
  </thead>
  <tbody>
<% foreach (Member m in Model)
   { %>
<tr><td><%= Html.Encode(m.LastName) %></td><td><%= Html.ActionLink<MembersController>(f => f.Detail(m.Id), Html.Encode(m.FirstName)) %></td></tr>
<% } %>
</tbody>
<tfoot>
<tr><th colspan="2"><%= Model.Count() %> eligible</th></tr></tfoot>
</table>

<h3>Email Addresses:</h3>
<%= Html.Encode(string.Join("; ", ((List<string>)ViewData["emails"]).ToArray())) %>
<script type="text/javascript">
  $(document).ready(function() {
    $("#list").tablesorter({ widgets: ['zebra'], headers: { 3: { sorter: 'link'}} });
  });
</script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="secondaryNav" runat="server">
</asp:Content>
