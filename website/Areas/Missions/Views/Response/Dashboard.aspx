<%@ Page Title="" Language="C#" MasterPageFile="~/Areas/Missions/Views/Response/Response.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container" style="width:100%; margin-left:auto; margin-right:auto; padding-right:30px">
  <div class="row">
    <div class="col-md-5">
      <h2>Response Dashboard</h2>
      <table class="table table-striped table-condensed">
        <thead>
          <tr><th>Mission</th><th>Started</th><th>Stage?</th><th>Call?</th></tr>
        </thead>
        <!-- ko ifnot: Missions.isLoading -->
        <tbody data-bind="foreach: Missions">
          <tr>
            <td data-bind="text: Mission.Title"></td>
            <td data-bind="text: Mission.StartTime.format('ddd MMM D')"></td>
            <td data-bind="text: ShouldStage"></td>
            <td data-bind="text: ShouldCall"></td>
          </tr>
        </tbody>
        <!-- /ko -->
        <!-- ko if: Missions.isLoading -->
        <tbody>
          <tr><td colspan="50">Loading ...</td></tr>
        </tbody>
        <!-- /ko -->
      </table>
    </div>
    <div class="col-md-7" style="border:solid 1px; height:400px; margin-top:2em" id="map"></div>
  </div>
</div>
<div class="container">
  <div data-bind="foreach: Missions">
    <div class="row">
      <h4 data-bind="text: Mission.Title"></h4>
      <div data-bind="ifnot: Responders().length">No active responders</div>
      <!-- ko if: Responders().length -->
      <div class="table-responsive">
        <table class="table table-striped table-condensed col-md-10">
        <thead>
          <tr><th>Name</th><th>Unit</th><th>Role</th><th>Status</th><th>Updated</th><th></th></tr>
        </thead>
        <tbody data-bind="foreach: Responders">
          <tr>
            <td><span data-bind="text: Responder.LastName"></span>, <span data-bind="text: Responder.FirstName"></span></td>
            <td data-bind="text:Unit.Name"></td>
            <td data-bind="text:Role"></td>
            <td data-bind="text:enumResponderStatus[Status].d"></td>
            <td data-bind="text:Updated.format('HH:mm ddd')"></td>
            <td><span data-bind="visible:Location">Map</span></td>
          </tr>
        </tbody>
      </table>
    </div>
      <!-- /ko -->
  </div>
  </div>
</div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
<script src="https://maps.googleapis.com/maps/api/js?sensor=false"></script>
<%= Scripts.Render("~/Areas/Missions/Scripts/Response/DashboardPage.js") %>
<script type="text/javascript">
  $(document).ready(function () {
    var model = new PageModel($.connection.responseHub.client, document.getElementById('map'));
    ko.applyBindings(model);
    model.load();
  });
</script>
</asp:Content>