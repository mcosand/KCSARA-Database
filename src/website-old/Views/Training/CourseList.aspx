<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<IEnumerable<TrainingCourseSummary>>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<div style="float:left;">
<% Html.BeginForm(); %>
  <fieldset>
    <label for="filter">Filter:</label>
    <%= Html.CheckBox("filter") + " Only show WACs and Badge courses" + Html.ValidationMessage("filter") %>
    <label for="unit">Show units:</label>
    <%= Html.DropDownList("unit", "All units")%>
    <label for="recent">&quot;Recently Expired&quot; is:</label>
    <%= Html.TextBox("recent", ViewData["recent"], new { @class = "input-box", style = "width:2em; display:inline;" }) + " months" + Html.ValidationMessage("recent")%>
    <label for="upcoming">&quot;Near Expiration&quot; is:</label>
    <%= Html.TextBox("upcoming", ViewData["upcoming"], new { @class = "input-box", style = "width:2em; display:inline;" }) + " months" + Html.ValidationMessage("upcoming") %>
    <%= Html.SubmitButton("Update", "Update", new { @class = "button", style="display:block; font-size:1em;"})%>
  </fieldset>
<% Html.EndForm(); %>
</div>

<table id="list" style="clear:both;" border="0" cellpadding="0" class="data-table">
  <thead><tr><th>Course Name</th><th>Past<br />Expiration</th><th>Recently<br />Expired</th><th>Near<br />Expiration</th><th style="padding:0 1em;">Current</th></tr></thead>
  <tbody>
  <% foreach(TrainingCourseSummary summary in ViewData.Model.OrderBy(x => x.Course.DisplayName) ) { %>
  <tr>
    <td><%= Html.ActionLink<TrainingController>(x => x.Current(summary.Course.Id, (Guid?)ViewData["unitFilter"], null), summary.Course.DisplayName) %></td>
    <td class="c"><%= summary.FarExpiredCount %></td>
    <td class="c"><%= summary.RecentCount %></td>
    <td class="c"><%= summary.UpcomingCount %></td>
    <td class="c" style="font-weight:bold;"><%= summary.CurrentCount %></td>
  </tr>
  <% } %>
  </tbody>
</table>
<script type="text/javascript">
  $(document).ready(function() {
  $("#list").tablesorter({ widgets: ['zebra'], headers: { 1: { sorter: 'link'}} });
  }
);
</script>
</asp:Content>