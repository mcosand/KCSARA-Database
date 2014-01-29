<%@ Page Title="" Language="C#" MasterPageFile="~/Areas/Missions/Views/Response/Response.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <div class="container" data-bind="visible: !CreatingMission()">
 <!-- <div data-bind="text: ko.toJSON($root)" /> -->
    
       <div class="list-group" style="padding-top: .4em" data-bind="foreach: MissionStatus">
      <a data-bind="attr: { href: '<%= Url.RouteUrl("MissionResponse_default", new { controller = "Response", action = "Info" }) %>/' + Mission.Id }" class="list-group-item">
        <span class="label label-warning pull-right" data-bind="visible: ShouldCall" style="margin-left:8px"><span class="glyphicon glyphicon-phone-alt"></span></span>
        <span class="label label-success pull-right" data-bind="visible: ShouldStage" style="margin-left:8px"><span class="glyphicon glyphicon-flag"></span></span>
        <div style="font-weight: bold" data-bind="text: Mission.Title"></div>
        <div class="pull-right" data-bind="text: Mission.StartTime.format('MMM D HH:mm')"></div>
        <div style="height:1em;" data-bind="text: Mission.StateNumber"></div>
      </a>
    </div>
    
    <a href="<%=  Url.RouteUrl("MissionResponse_default", new { controller = "Response", action = "Create" }) %>" class="list-group-item">
      <div style="font-weight: bold; padding: 2px 0px">Create New Mission</div>
    </a>
  </div>
  <!-- /.container -->
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
  <script type="text/javascript">
    var PageModel = function (missionInit) {
      var self = this;
      $.each(missionInit, function (i, m) { mapMoment(m.Mission, ['StartTime']); });
      this.MissionStatus = ko.observableArray(missionInit);
      this.CreatingMission = ko.observable(false);

      this.ShowBack = ko.observable(null);
      this.Menu = ko.observableArray([{ title: 'foo', target: '#blah' }, { title: 'bar', target: '#other' }]);
    };

    $(document).ready(function () {
      var model = new PageModel(<%= Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.Data) %>);
      ko.applyBindings(model);
    });
  </script>
</asp:Content>
