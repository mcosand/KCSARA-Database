<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Kcsara.Database.Web.Controllers.MapDataRequests>" %>

<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<asp:Content ID="Content1"
  ContentPlaceHolderID="MainContent" runat="server">
  <% if (!string.IsNullOrWhiteSpace(Model.Title)) { %>
  <h2><%: Model.Title %></h2>
  <% } %>
  <div id="msgs"></div>
  <% 
    if ((Model.Modes & MapModes.MemberHouses) == MapModes.MemberHouses)
     { %>
    <div id="memberMap">Show members of: <%: Html.DropDownList("unitSelect", (SelectList)ViewData["units"], "All Units")%> <button id="loadHouses" onclick="loadHouses(); return false;" >Show</button></div>
  <% } %>
  <div style="font-size:75%"><input type="checkbox" id="showTopo" onchange="return toggleTopo(this.checked);" /> Show Topo</div>
  <div id='myMap' style="position: relative; width: 100%; height: 600px;">
  </div>

  <script type="text/javascript">
      jQuery(function () {
          var interval = setInterval(function () {
              if ((eval("typeof VEMap") != "undefined")
&& (document.getElementById("myMap").attachEvent != undefined))
              { clearInterval(interval); GetMap(); }
          }, 10);
      });
  </script>

</asp:Content><asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">

  <script type="text/javascript" src="https://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=6.2&s=1"></script>
  <%
    StringBuilder requests = new StringBuilder();
    foreach (var s in Model.Missions)
    {
      requests.AppendFormat("LoadMission('{0}');", s);
    }

    if ((Model.Modes & MapModes.MissionBrowser) == MapModes.MissionBrowser)
    {
      requests.AppendLine("$.post(appRoot+'missions/GetGeographies', { start: null, stop: null, overview: true }, function(data, textStatus) { renderMapDataView(data); }, 'json');");
    }
  %>

<%=  "<script type=\"text/javascript\">" %>
    var map = null;
    var appRoot = '<%= ViewData["AppRoot"] %>';
    
    iconSet = [ 'pls', 'found', 'base', 'team', 'clue', 'm_res', 'default' ];
    icons = new Array();
    for (var i in iconSet)
    {
      var id = iconSet[i];
      icons[id] = new VECustomIconSpecification();
      icons[id].TextContent = "";
      icons[id].Image = appRoot+"Content/images/maps/" + id + ".png";
      icons[id].ImageOffset = new VEPixel(3,3);
    }

    function GetMap() {
      map = new VEMap('myMap');

      map.LoadMap(new VELatLong(<%: Model.CenterLat %>, <%: Model.CenterLong %>), <%: Model.Zoom %>, 'r', false);
      var bounds = [
   new VELatLongRectangle(new VELatLong(49,-125.0),
                          new VELatLong(44,-118.0))];
    var layerID = "topo";
    var tileSourceSpec = new VETileSourceSpecification(layerID, "http://maps.mytopo.com/groundspeak/tilecache.py/1.0.0/topoG/{12}/{13}/{14}.png");
    tileSourceSpec.GetTilePath = GetTopoTilePath;
    tileSourceSpec.Bounds     = bounds;
    tileSourceSpec.MinZoom    = 8;
    tileSourceSpec.MaxZoom    = 30;
    tileSourceSpec.Opacity    = 0.8;
    tileSourceSpec.ZIndex     = 59;
    map.AddTileLayer(tileSourceSpec, false);
    toggleTopo(document.getElementById('showTopo').checked);

      <%= requests.ToString() %>
 }

<% if ((Model.Modes & MapModes.MemberHouses) == MapModes.MemberHouses) 
    { %>

var houses = new Array();
function loadHouses()
{
  var unit = $('#unitSelect').val();
  
  while (houses.length > 0)
  {
    map.DeleteShape(houses[0]);
    houses.shift();
  }
  $.post(appRoot+'members/GetGeographies', { unitId: (unit == "") ? null : unit }, function(data, textStatus) {
  
  renderMapDataView(data, houses); }, 'json');
}
<% } %>

var defaultLine = new VEColor(255,69,0,1.0);
var lineColors = [defaultLine, new VEColor(255,66,195,1.0), new VEColor(144,76,255,1.0)];

 function renderMapDataView(data, shapeStore)
 {
   var doStore = false;
   if (shapeStore) { doStore = true; while (shapeStore.length) shapeStore.pop(); }
    var shape;
    for (var i in data.Items)
    {
      var g = data.Items[i];

      var shape;
      if (g.__type == "RouteView")
      {
        var pts = new Array();
        for (var j in g.Points)
        {
          pts[j] = new VELatLong(g.Points[j].Lat, g.Points[j].Long);
        }
        shape = new VEShape(VEShapeType.Polyline, pts);
        shape.HideIcon();
        
        var color = lineColors.shift();
        if (color == undefined) color = defaultLine;
        shape.SetLineColor(color);
        
        map.AddShape(shape);
        if (doStore) shapeStore.push(shape);

        shape = new VEShape(VEShapeType.Pushpin, new VELatLong(g.Points[0].Lat, g.Points[0].Long));
        shape.SetCustomIcon((icons[g.Kind] == undefined) ? icons["default"] : icons[g.Kind]);
      }
      else
      {
        shape = new VEShape(VEShapeType.Pushpin, new VELatLong(g.Lat, g.Long));
        shape.SetCustomIcon((icons[g.Kind] == undefined) ? icons["default"] : icons[g.Kind]);
      }
      shape.SetTitle(g.Description);
      map.AddShape(shape);
      if (doStore) shapeStore.push(shape);
    }
    for (var i in data.Messages)
    {
      $('#msgs').append($('<p>'+data.Messages[i]+'</p>'));
    }
 }

 function LoadMission(id)
 {
    $.post(appRoot+'missions/GetGeography', { id: id, overview: false }, function(data, textStatus) {
        renderMapDataView(data);
    }, "json");
 }

function toggleTopo(checked)
{
  if (checked)
  {
    map.ShowTileLayer("topo");
  }
  else
  {
    map.HideTileLayer("topo");
  } 
}
function GetTopoTilePath(data)
{
  return "http://maps.mytopo.com/groundspeak/tilecache.py/1.0.0/topoG/" + data.ZoomLevel + "/" + data.XPos + "/" + data.YPos + ".png";
}
<%=  "</script>" %>

</asp:Content><asp:Content ID="Content3" ContentPlaceHolderID="secondaryNav" runat="server">
</asp:Content>