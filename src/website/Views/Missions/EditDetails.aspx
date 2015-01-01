<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true"
  Inherits="System.Web.Mvc.ViewPage<MissionDetails>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
<style type="text/css">
.il { display:inline; }
</style>

<% Html.BeginForm(); %>
<div style="clear:both; margin-top:.5em;">
  <table>
  <tr><th colspan="2">Weather</th></tr>
  <tr><td><label for="Clouds">Clouds</label></td>
      <td><%= Html.CheckBoxList("Clouds", (MultiSelectList)ViewData["Clouds"], "il", null) + Html.ValidationMessage("Clouds") %>
      </td></tr>
  <tr><td><label for="Visibility">Visibility (feet)</label></td>
      <td><%= Html.EditorFor(f => f.Visibility, new { @class = "input-box", style="display:inline; width:4em;"}).ToString() + Html.ValidationMessage("Visibility") %>
      </td></tr>
  <tr><td><label>Temperature (&deg;F)</label></td>
      <td>High: <%= Html.EditorFor(f => f.TempHigh, new { @class = "input-box", style = "display:inline; width:4em;" }).ToString() + Html.ValidationMessage("TempHigh")%>
      Low: <%= Html.EditorFor(f => f.TempLow, new { @class = "input-box", style = "display:inline; width:4em;" }).ToString() + Html.ValidationMessage("TempLow")%>
      </td></tr>
  <tr><td><label>Winds (mph)</label></td>
      <td>High: <%= Html.EditorFor(f => f.WindHigh, new { @class = "input-box", style = "display:inline; width:4em;" }).ToString() + Html.ValidationMessage("WindHigh")%>
      Low: <%= Html.EditorFor(f => f.WindLow, new { @class = "input-box", style = "display:inline; width:4em;" }).ToString() + Html.ValidationMessage("WindLow")%>
      </td></tr>
  <tr><td><label>Rain</label></td>
      <td><%= Html.EnumToDropDown(typeof(RainType), "RainType", Model.RainType, "Unknown") + Html.ValidationMessage("RainType")
                  + Html.EditorFor(f => f.RainInches, new { @class = "input-box", style = "display:inline; width:4em;" })%> inches <%= Html.ValidationMessage("RainInches") %>
      </td></tr>
  <tr><th colspan="2">Terrain</th></tr>
  <tr><td><label for="Terrain">Terrain</label></td>
      <td><%= Html.EnumToDropDown(typeof(Terrain), "Terrain", Model.Terrain, "Unknown") + Html.ValidationMessage("Terrain") + Html.ValidationMessage("Terrain")%>
      </td></tr>
  <tr><td><label for="Topography">Topography</label></td>
      <td><%= Html.EnumToDropDown(typeof(Topography), "Topography", Model.Topography, "Unknown") + Html.ValidationMessage("Topography") + Html.ValidationMessage("Topography")%>
      </td></tr>
  <tr><td><label for="GroundCoverDensity">Ground Cover</label></td>
      <td><%= Html.EnumToDropDown(typeof(Density), "GroundCoverDensity", Model.GroundCoverDensity, "Unknown") + Html.ValidationMessage("GroundCoverDensity")%>
      <%= Html.EditorFor(f => f.GroundCoverHeight, new { @class = "input-box", style = "display:inline; width:4em;" })%> inches <%= Html.ValidationMessage("GroundCoverHeight") %>
      </td></tr>
 
  <tr><td><label for="TimberType">Timber</label></td>
      <td><%= Html.EnumToDropDown(typeof(Density), "TimberType", Model.TimberType, "Unknown") + Html.ValidationMessage("TimberType")%>
      </td></tr>
  
  <tr><td><label for="WaterType">Water</label></td>
      <td><%= Html.EnumToDropDown(typeof(Water), "WaterType", Model.WaterType, "Unknown") + Html.ValidationMessage("WaterType")%>
      </td></tr>
  
  <tr><td><label>Elevation (feet)</label></td>
      <td>High: <%= Html.EditorFor(f => f.ElevationHigh, new { @class = "input-box", style = "display:inline; width:4em;" }).ToString() + Html.ValidationMessage("ElevationHigh")%>
      Low: <%= Html.EditorFor(f => f.ElevationLow, new { @class = "input-box", style = "display:inline; width:4em;" }).ToString() + Html.ValidationMessage("ElevationLow")%>
      </td></tr>

  <tr><th colspan="2">Response</th></tr>
  <tr><td colspan="2">
    <table width="100%">
      <tr><td><label>Tactics</label></td><td><label>Clues Found By</label></td><td><label>Termination</label></td></tr>
      <tr><td><%= Html.CheckBoxList("TacticsList", (MultiSelectList)ViewData["TacticsList"]) %>
          <div class="fields"><input type="checkbox" id="chkTact" /><%= Html.TextBox("TacticsOther", ViewData["TacticsOther"], new { @class="input-box il" })%></div>
          <%= Html.ValidationMessage("Tactics") %>
          </td>
          <td><%= Html.CheckBoxList("CluesMethodList", (MultiSelectList)ViewData["CluesMethodList"])%>
          <div class="fields"><input type="checkbox" id="Checkbox1" /><%= Html.TextBox("CluesMethodOther", ViewData["CluesMethodOther"], new { @class="input-box il" })%></div>
          <%= Html.ValidationMessage("CluesMethod") %>
          </td>
          <td><%= Html.CheckBoxList("TerminateReasonList", (MultiSelectList)ViewData["TerminateReasonList"])%>
          <div class="fields"><input type="checkbox" id="Checkbox2" /><%= Html.TextBox("TerminateReasonOther", ViewData["TerminateReasonOther"], new { @class="input-box il" })%></div>
          <%= Html.ValidationMessage("TerminateReason") %>
          </td></tr>
    </table>
    <div><%= Html.CheckBox("Debrief") %>Debrief Completed? <%= Html.ValidationMessage("Debrief") %></div>
    <div><%= Html.CheckBox("Cisd") %>CISD Required? <%= Html.ValidationMessage("Cisd") %></div>
    </td></tr>
</table>
</div>
<input type="submit" value="Save" />
<% Html.EndForm(); %>
</asp:Content>
