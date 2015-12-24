<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ExpandedRowsContext>" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<% Html.RenderPartial("EventRoster", Model); %>

<p>The information above allows you to review what was parsed out of the Excel document. It may change if you merge it with an existing <%= Model.Type.ToString().ToLower() %>.</p>
<form method="post" action="<%= Url.Action("Submit4x4Roster") %>" >
<h3>Apply to <%= Model.Type %>:</h3>
<table>
<%  bool selected = false;
    foreach (var m in (IRosterEvent[])ViewData["alternateMissions"]) { %>
<tr><td><input type="radio" name="missionId" value="<%: m.Id %>" <%= selected ? "" : "checked=\"checked\"" %> /></td>
<td><%: m.StateNumber %></td>
<td><%: m.StartTime %></td>
<td><%: m.Title %></td></tr>

<% selected = true;
    } %>
<tr><td><input type="radio" name="missionId" value="<%: Model.EventId %>" <%= selected ? "" : "checked=\"checked\"" %> /></td><td colspan="3">Use information from Excel file</td></tr>
</table>

<input type="submit" value="Submit Roster" />
</form>

</asp:Content>
