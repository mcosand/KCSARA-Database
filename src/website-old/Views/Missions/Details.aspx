<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Missions/Missions.Master"
  Inherits="System.Web.Mvc.ViewPage<MissionDetails>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<style type="text/css">
.il { display:inline; }
</style>
  <% bool isAdmin = Page.User.IsInRole("cdb.admins"); %>
  <% Html.RenderPartial("Mission", Model.Mission);
     var inputBox = new { @class = "input-box" };
     var ilBox = new { @class = "input-box", style="display:inline;" };
     %>
  <h2>
    Mission Details</h2>
  <div style="position: relative;">
    <%= this.ModelData(Model) %>
  </div>

  <% if (Page.User.IsInRole("cdb.missioneditors"))
          Response.Write(Html.PopupActionLink<MissionsController>(f => f.EditDetails(Model.Mission.Id), "Edit...", 1000, 700)); %>

  <table>
   <tr><th colspan="2">Weather</th></tr>
   <tr><td>Clouds</td><td><%= Html.Encode(Model.Clouds) %></td></tr>
   <tr><td>Visibility:</td><td><%= Model.Visibility.HasValue ? string.Format("{0}'", Model.Visibility) : "" %></td></tr>
   <tr><td>Temperature:</td><td><%= string.Format("{0} - {1} &deg;F", Model.TempLow.HasValue ? Model.TempLow.ToString() : "??", 
                                                                     Model.TempHigh.HasValue ? Model.TempHigh.ToString() : "??")%></td></tr>
    
    <tr><td>Winds</td><td><%= string.Format("{0} - {1} mph", Model.WindLow.HasValue ? Model.WindLow.ToString() : "??", 
                                                                     Model.WindHigh.HasValue ? Model.WindHigh.ToString() : "??")%></td></tr>
    <tr><td>Rain</td><td><%= (Model.RainType.HasValue ? ((RainType)Model.RainType.Value).ToString() : "")  +
                           ((Model.RainType.HasValue && Model.RainInches.HasValue) ? " - " : "") + 
                           (Model.RainInches.HasValue ? (Model.RainInches.ToString() + "&quot;") : "") %> </td></tr>

    <tr><td>Snow</td><td><%= (Model.SnowType.HasValue ? ((SnowType)Model.SnowType.Value).ToString() : "")  +
                           ((Model.SnowType.HasValue && Model.SnowInches.HasValue) ? " - " : "") + 
                           (Model.SnowInches.HasValue ? (Model.SnowInches.ToString() + "&quot;") : "") %> </td></tr>
                       
    <tr><th colspan="2">Terrain</th></tr>
    <tr><td>Terrain</td><td><%= Model.Terrain.HasValue ? ((Terrain)Model.Terrain.Value).ToString() : "" %></td></tr>
    <tr><td>Topography</td><td><%= Model.Topography.HasValue ? ((Topography)Model.Topography.Value).ToString() : "" %></td></tr>
    <tr><td>Ground Cover</td><td><%= (Model.GroundCoverDensity.HasValue ? ((Density)Model.GroundCoverDensity.Value).ToString() : "")  +
                           ((Model.GroundCoverDensity.HasValue && Model.GroundCoverHeight.HasValue) ? " - " : "") + 
                           (Model.GroundCoverHeight.HasValue ? (Model.GroundCoverHeight.ToString() + "&quot;") : "") %> </td></tr>
    <tr><td>Timber</td><td><%= Model.TimberType.HasValue ? ((Density)Model.TimberType.Value).ToString() : "" %></td></tr>
    <tr><td>Water</td><td><%= Model.TimberType.HasValue ? ((Density)Model.TimberType.Value).ToString() : "" %></td></tr>
    <tr><td>Elevation</td><td><%= string.Format("{0}' - {1}'", Model.ElevationLow.HasValue ? Model.ElevationLow.ToString() : "??", 
                                                                     Model.ElevationHigh.HasValue ? Model.ElevationHigh.ToString() : "??")%></td></tr>

  </table>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
