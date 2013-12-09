<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Missions/Missions.Master" Inherits="System.Web.Mvc.ViewPage<MapDataView>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsar.Database" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<% bool canEdit = Page.User.IsInRole("cdb.missioneditors"); %>
<% Html.RenderPartial("Mission", ViewData["mission"]); %>

<h2>Geography</h2>

  <p><%= Html.ActionLink<MapController>(f => f.Index(Model.Id), "Show Map ...", new { target = "_blank" })  %></p>
  <table class="data-table" id="table">
  <thead><tr><th>Type</th><th>Label</th><th>Lat/Long</th><th>Length (mi)</th><th>Time</th></tr></thead>
  <tbody>
  <tr><td colspan="10" style="text-align:center;">Loading ...</td></tr>
  </tbody>
  </table>
  <% if (canEdit) { %><button id="addwaypoint" class="button">Add Marker</button> <%= Html.PopupActionButton<MissionsController>(f => f.UploadGpx(Model.Id), "Upload GPX", 450, 450) %><% } %>

<div id="waypointform" title="Waypoint Entry">
    <p class="validateTips"></p>

    <form action="#">
    <fieldset>
        <label for="type">Type</label>
        <select id="type" name="type" class="ui-corner-all" onchange="updateInstance(this.value)">
          <option value="base">Base Camp</option>
          <option value="team">Team Position ...</option>
          <option value="clue">Clue</option>
          <option value="cluLkp">Last Known Place (LKP)</option>
          <option value="found">Subject Found</option>
          <option value="extract">Subject Extracted</option>
        </select><br />
        <select id="instance" name="instance" class="ui-corner-all" disabled="disabled">
        </select>

        <label for="time">Date/Time</label>
        <input type="text" name="time" id="time" class="text ui-corner-all datetimepicker" />
        
        <label for="desc">Description</label>
        <input type="text" name="desc" id="desc" class="text ui-corner-all" />

        <label for="lat">Coordinates (WGS 84)</label>
        Latitude: N <input type="text" name="lat" id="lat" class="text ui-corner-all" /><br />
        Longitude: W <input type="text" name="lon" id="lon" class="text ui-corner-all" />
    </fieldset>
    </form>
</div>
<%= "<script type=\"text/javascript\">" %>
var geoForm;
var coordDisp = '<%= ViewData["coordDisplay"] %>';
$(document).ready(function () {
  var types = ['<%= string.Join("', '", PersonContact.AllowedTypes)  %>'];
  for (var t in types) {
    $("#contactType").append("<option>" + types[t] + "</option>");
  }

  geoForm = new ModelTable("#table", "#waypointform");
  geoForm.height = 340;
  <%= canEdit ? "geoForm.canEdit = true;" : "" %>
  geoForm.getUrl = '<%= Url.Action("GetGeography", new { id = Model.Id }) %>';
  geoForm.deleteUrl = '<%= Url.Action("DeleteWaypoint") %>';
  geoForm.postUrl = '<%= Url.Action("SubmitWaypoint") %>';
  geoForm.unpacker = function(data) { return data.Items; };
  geoForm.objectType = "Waypoint";
  geoForm.dateTimeFields = ['Time'];
  geoForm.renderer = function (data, row) {
        $(row).html('<td>'+ifDefined(data.Kind)+
        '</td><td>'+ifDefined(data.Description)+
        '</td><td>'+waypointLocation(data)+
        '</td><td>'+ (isNaN(data.Len) ? "" : (data.Len/1609.344).toFixed(2)) +
        '</td><td>'+((data.Time == undefined) ? "" : formatDateTime("yy-mm-dd HH:ii", data.Time)) +
        '</td>' 
        )};
  geoForm.formParser = function(c) {
    c.Kind = $('#type').val();
    c.Description = $('#desc').val();
    var t = $('#time').val();
    var d = Date.parse(t);
    if (t == "") { delete c.Time; } else { c.Time = d; }

    d = parseCoordinate($('#lat').val());
    c.Lat = d;

    d = -Math.abs(parseCoordinate($('#lon').val()));
    c.Long = d;
  
    if ($('#instance').val()) { c.InstanceId = $('#instance').val(); } else { delete c.InstanceId; }
    c.EventId = '<%: ViewData["missionId"] %>';
    return c;
  };
  geoForm.fillForm = function(item) {
      if (item['Id'] != null && item.__type != "WaypointView")
      { alert('Can only edit waypoints'); return false; }
      
      $('#type').val(item.Kind);
      if (!geoForm.updateInstance(item.Kind, item.InstanceId)) return false;
      if (item.Time != undefined) $('#time').val(formatDateTime('yy-mm-dd HH:ii', item.T));
      $('#desc').val(item.Description);
      $('#lat').val((item['Lat'] == null) ? '' : formatCoordinate(item.Lat, false));
      $('#lon').val((item['Long'] == null) ? '' : formatCoordinate(item.Long, false));
      return true;
      };

  geoForm.updateInstance = function(kind, selected)
  {
    var result = false;
    $.ajax({ type: 'POST', url: kcsarBaseUrl + 'missions/getgeographyinstances', data: { missionId: '<%: ViewData["missionId"] %>', kind: kind }, dataType: 'json', async:false,
      success: function (data) {
        if (data.Errors.length == 0) {
          var html = "";
          $('#instance').attr('disabled', 'disabled');
          for (var i in data.Result)
          {
            html += '<option value="'+data.Result[i].Key+'"';
            if (selected != undefined && data.Result[i].Key == selected) html += ' selected="selected"';
            html += '>'+data.Result[i].Value+'</option>';
            $('#instance').removeAttr('disabled');
          }        
          $('#instance').html(html);
          result = true;
        }
      },
      error: function (request, status, error) {
        $('#waypointform').dialog('close');
        handleDataActionError(request, status, error);
      }
    });
    return result;
  }

  geoForm.Initialize();

  $('#addwaypoint').click(function () {
      var newObj = new Object();
      geoForm.CreatePrompt(newObj);
    });
});

function formatCoordinate(coord, fancy)
{
  coord = Math.abs(coord);
  if (coordDisp == 'DecimalDegrees')
  {
    return coord.toFixed(5) + (fancy ? '&deg;' : '');
  }
  else if (coordDisp == 'DecimalMinutes')
  {
    return Math.floor(coord) + (fancy ? '&deg; ' : ' ') + ((coord - Math.floor(coord)) * 60.0).toFixed(4) + (fancy ? '&#039' : '');
  }
  else return "Unknown Format";
}

function waypointLocation(data)
{
  if (data.Lat == undefined || data.Long == undefined)
  {
    return "";
  }

  return "N " + formatCoordinate(data.Lat, true) + ", W "+ formatCoordinate(data.Long, true);
}
<%= "</script>" %>
</asp:Content>