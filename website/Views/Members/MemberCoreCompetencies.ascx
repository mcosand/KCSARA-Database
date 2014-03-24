<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<Guid>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>
<table class="data-table" id="ccTable">
  <thead><tr><th>Competency</th><th>Classroom</th><th>Written</th><th>Performance</th></tr></thead>
  <tbody>
    <tr><th>ICS-100</th><%: CCStatus("ICS-100", true) %></tr>
    <tr><th>ICS-700</th><%: CCStatus("ICS-700", true) %></tr>
    <tr><th>Core/Clues</th><td></td><%: CCStatus("Core/Clues.WE", false) %><td></td></tr>
    <tr><th>Core/CPR</th><%: CCStatus("Core/CPR", true) %></tr>
    <tr><th>Core/Crime</th><%: CCStatus("Core/Crime.C", false) %><%: CCStatus("Core/Crime.WE", false) %><td></td></tr>
    <tr><th>Core/FirstAid</th><%: CCStatus("Core/FirstAid", true) %></tr>
    <tr><th>Core/GPS</th><td></td><%: CCStatus("Core/GPS.WE", false) %><%: CCStatus("Core/GPS.PE", false) %></tr>
    <tr><th>Core/Helo</th><%: CCStatus("Core/Helo.C", false) %><%: CCStatus("Core/Helo.WE", false) %><td></td></tr>
    <tr><th>Core/Legal</th><%: CCStatus("Core/Legal.C", false) %><%: CCStatus("Core/Legal.WE", false) %><td></td></tr>
    <tr><th>Core/Management</th><td></td><%: CCStatus("Core/Management.WE", false) %><td></td></tr>
    <tr><th>Core/Nav</th><td></td><%: CCStatus("Core/Nav.WE", false) %><%: CCStatus("Core/Nav.PE", false) %></tr>
    <tr><th>Core/Radio</th><td></td><%: CCStatus("Core/Radio.WE", false) %><%: CCStatus("Core/Radio.WE", false) %></tr>
    <tr><th>Core/Rescue</th><td></td><%: CCStatus("Core/Rescue.WE", false) %><%: CCStatus("Core/Rescue.WE", false) %></tr>
    <tr><th>Core/Safety</th><td></td><%: CCStatus("Core/Safety.WE", false) %><%: CCStatus("Core/Safety.WE", false) %></tr>
    <tr><th>Core/Search</th><td></td><%: CCStatus("Core/Search.WE", false) %><%: CCStatus("Core/Search.WE", false) %></tr>
    <tr><th>Core/Survival</th><td></td><%: CCStatus("Core/Survival.WE", false) %><%: CCStatus("Core/Survival.WE", false) %></tr>
  </tbody>
</table>
<script runat="server">
  protected MvcHtmlString CCStatus(string name, bool span)
  {
    var ccStatus = (Dictionary<string, TrainingStatus>)ViewData["CoreStatus"];
    string s = span ? " colspan=\"3\" class=\"c\"" : "";
    
    TrainingStatus status;
    if (!ccStatus.TryGetValue(name, out status))
    {
      return new MvcHtmlString("<td" + s + ">Unknown</td>");
    }
    
    if (status.Completed == null)
    {
      return new MvcHtmlString("<td" + s + " class=\"exp_Missing\">Missing</td>");
    }

    if (!status.Expires.HasValue)
    {
      return new MvcHtmlString("<td" + s + ">Complete</td>");
    }

    return new MvcHtmlString(string.Format("<td{0} class=\"exp_{1}\">{2:yyyy-MM-dd}</td>",
      s, (status.Expires < DateTime.Today) ? "Expired" : "NotExpired", status.Expires));
  }
</script>