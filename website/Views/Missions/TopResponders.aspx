<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<Dictionary<RosterSummaryRow, Member>>" %>

<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
<style>
.r { text-align:right; }</style>
  <script type="text/javascript" src="<%= this.ResolveUrl("~/Content/script/ui.core.js") %>" > </script>
  <script type="text/javascript" src="<%= this.ResolveUrl("~/Content/script/ui.datepicker.js") %>" > </script>
  <link href="<%=this.ResolveUrl("~/Content/jquery-ui-1.7.custom.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<% using (Html.BeginForm()) { %>
<fieldset>
    <label for="start">From:</label>
    <%= Html.TextBox("start", string.Format("{0:yyyy-MM-dd}", TempData["start"]), new { @class = "input-box", style = "display:inline;" })%><%= Html.ValidationMessage("start") %><br />
    <label for="start">Until:</label>
    <%= Html.TextBox("end", string.Format("{0:yyyy-MM-dd}", TempData["end"]), new { @class = "input-box", style = "display:inline;" })%><%= Html.ValidationMessage("end") %><br />

    <label for="unit">Unit:</label>
    <%= Html.DropDownList("unit", "All Units").ToString() + Html.ValidationMessage("unit")%>
    
    <input type="submit" value="Save" class="button" />

</fieldset>
<% } %>

<table>
  <thead>
    <tr><th></th><th>Name</th><th>Hours</th><th>Missions</th><th>Miles</th></tr>
  </thead>
  <tbody>
  <% int i = 1;
    foreach (RosterSummaryRow row in Model.Keys.OrderByDescending(f => f.Hours))
     { %>
    <tr><td><%= i++ %></td><td><%= Html.ActionLink<MembersController>(f => f.Detail(Model[row].Id), Model[row].ReverseName) %><%= (Model[row].BirthDate.HasValue && Model[row].BirthDate.Value.AddYears(18) >= DateTime.Today) ? "*" : "" %></td><td><%= row.Hours.ToString("0.0") %></td><td><%= row.Missions %></td><td><%= row.Miles %></td></tr>
  <%} %>
  </tbody>
</table>
<script>
    $(function() {
      $('#start').datepicker({
        dateFormat: 'yy-mm-dd',
        changeMonth: true, changeYear: true,
        showOn: 'button', buttonImage: '<%=this.ResolveUrl("~/Content/images/calendar.gif") %>', buttonImageOnly: true
      });
      $('#unit').datepicker({
        dateFormat: 'yy-mm-dd',
        changeMonth: true, changeYear: true,
        showOn: 'button', buttonImage: '<%=this.ResolveUrl("~/Content/images/calendar.gif") %>', buttonImageOnly: true
      });
    })
</script>
</asp:Content>
