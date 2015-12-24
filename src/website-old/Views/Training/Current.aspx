<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<IEnumerable<ComputedTrainingAward>>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% TrainingCourse course = (TrainingCourse)ViewData["Course"]; %>
    <h1><%= course.DisplayName %></h1>
    <%= Html.PopupActionLink<TrainingController>(x => x.EditCourse(course.Id), "Properties...", 600) %>

    
    <h2>Current Members:</h2>
    <p>This list only shows members that are active with at least one unit.</p>
    <div style="float:left;">
    <% Html.BeginForm(); %>
  <fieldset>
    <%= Html.CheckBox("expired") + " Show expired entries" + Html.ValidationMessage("expired") %>
    <label for="unit">Show units:</label>
    <%= Html.DropDownList("unit", "All units")%>
    <%= Html.SubmitButton("Update", "Update", new { @class = "button", style="display:block; font-size:1em;"})%>
  </fieldset>
<% Html.EndForm(); %>
    </div>
    <table border="0" cellpadding="0"  style="clear:both;" class="data-table" id="t">
    <thead>
<tr><th>Member</th><th>Completed</th><th>Expires</th><th>Via Rule</th></tr>
</thead>
<tbody>
<%
  foreach (ComputedTrainingAward ta in ViewData.Model)
  { %>
    <tr>
  <td><%= Html.ActionLink<MembersController>(x => x.Detail(ta.Member.Id), ta.Member.ReverseName)%></td>
  <td><%= string.Format("{0:yyyy-MM-dd}", ta.Completed) %></td>
  <td><%= string.Format("{0:yyyy-MM-dd}", ta.Expiry)%></td>
  <td><%= ta.Rule != null%></td></tr>
<% } %>
</tbody>
<tfoot>
<tr><th colspan="4">Total Members: <%= ViewData.Model.Count() %></th></tr></tfoot>
</table>
<script>
  $(document).ready(function() {
    $("#t").tablesorter({ widgets: ['zebra'], headers: { 1: { sorter: 'link'}} });
  });
</script>
</asp:Content>
