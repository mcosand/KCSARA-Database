<%@ Page Title="" Language="C#" MasterPageFile="~/Areas/Missions/Views/Response/Response.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <div class="container">
    <h4 data-bind="text:Info().Mission.Title"></h4>
    <div><%= User.Identity.Name %></div>
    <!-- ko ifnot:EditResponse -->
    <div>Next operational period starts <span data-bind="text:Info().NextStart.fromNow()"></span>.</div>
    <!-- ko if:Info().StopStaging -->
    <div>Current period stops <span data-bind="text:Info().StopStaging.fromNow()"></span></div>
    <!-- /ko -->
    <div>Responding Units:
      <ul data-bind="foreach: Info().ActiveUnits">
        <li data-bind="text:Name" />
      </ul>
    </div>
    <button class="btn-lg btn-block" data-bind="click: startEdit, css:{ 'btn-success':MyUnitResponding, 'btn-warning':!MyUnitResponding()}">Respond</button>
  <!-- /ko -->
  <!-- ko if:EditResponse -->
    <form class="form-horizontal" role="form" style="display:none" data-bind="visible:EditResponse, with:EditResponse">
      <div class="form-group"  data-bind="css: { 'has-error': Status.invalid }">
        <label for="editStatus" class="col-sm-3 control-label">Status</label>
        <div class="col-sm-9">
          <select id="editStatus" data-bind="value:Status, options:enumResponderStatus._names, optionsText:function(i){return enumResponderStatus[i].d;}, event: {blur:Status.validate}, hasFocus: Status.hasFocus" class="form-control">
          </select>
          <div class="help-block" data-bind="foreach:Status.errors, visible:Status.invalid">
            <div class="text-danger" data-bind="text:$data"></div>
          </div>
        </div>
      </div>
      <div class="form-group"  data-bind="css: { 'has-error': Role.invalid }">
        <label for="editRole" class="col-sm-3 control-label">In Role</label>
        <div class="col-sm-9">
          <select id="editRole" data-bind="value:Role, options:enumMissionRole._names, optionsText:function(i){return enumMissionRole[i].d;}, event: {blur:Role.validate}, hasFocus: Role.hasFocus" class="form-control">
          </select>
          <div class="help-block" data-bind="foreach:Role.errors, visible:Role.invalid">
            <div class="text-danger" data-bind="text:$data"></div>
          </div>
        </div>
      </div>
      <div class="form-group">
        <label for="editUnit" class="col-sm-3 control-label">With Unit</label>
        <div class="col-sm-9">
          <select id="editUnit" data-bind="value: Unit, options: Unit.options, optionsText:'Name', optionsAfterRender: setOptionDisable" class="form-control"></select>
          <div class="alert alert-warning" data-bind="visible:!Unit().IsActive"><strong><span data-bind="text:Unit().Name"></span> hasn't responded!</strong> Continuing will activate the unit for the mission.
            This may be against your unit's working guidelines.
          </div>
        </div>
 <!--       <label for="createTitle" class="col-sm-3 control-label">Mission Title</label>
        <div class="col-sm-9">
          <input type="text" id="createTitle" data-bind="value:Title, event: {blur:Title.validate}, hasFocus: Title.hasFocus" class="form-control" placeholder="Required"/>
          <div class="help-block" data-bind="foreach:Title.errors, visible:Title.invalid">
            <div class="text-danger" data-bind="text:$data"></div>
          </div>
        </div>
 -->
      </div>
      <div class="form-group">
        <label for="editLocate" class="col-sm-3 control-label">Location</label>
        <div class="col-sm-9">
          <div><input type="radio" name="editLocate" value="geo" data-bind="checked:Location.Type, disable: Location.GeoReason"/> Use current location <span data-bind="text: Location.GeoReason"></span></div>
          <div><input type="radio" name="editLocate" value="none" data-bind="checked:Location.Type" /> Don't send location</div>
        </div>
        <div data-bind="text: ko.toJSON($data)"></div>
      </div>
<!--      <div class="form-group" data-bind="css: { 'has-error': Eta.invalid }">
        <label for="editEta" class="col-sm-3 control-label" data-bind="text:Eta.Label"></label>
        <div class="col-sm-9">
          <input type="text" id="editEta" data-bind="value:Eta, event: {blur:Eta.validate}, hasFocus: Eta.hasFocus" class="form-control" placeholder=""/>
          <div class="help-block" data-bind="foreach:Eta.errors, visible:Eta.invalid">
            <div class="text-danger" data-bind="text:$data"></div>
          </div>          
        </div>
      </div>
  -->
      <input type="submit" class="btn-lg btn-block btn-primary" data-bind="value:'I\'m ' + enumResponderStatus[Status()].d, click:$root.postResponse, disable:$root.IsWorking" />
     </form>
  <!-- /ko -->
    </div><!-- /.container -->
  <div class="container">

  </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
<script type="text/javascript">
</script>
<%= Scripts.Render("~/Areas/Missions/Scripts/Response/InfoPage.js") %>
<script type="text/javascript">
  $(document).ready(function () {
    var model = new PageModel(<%= Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.Data) %>, <%= Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.MyUnits) %>, '<%=  Url.RouteUrl(Kcsara.Database.Web.Areas.Missions.api.ResponseApiController.RouteName, new { httproute="", controller = "ResponseApi", action = "Checkin", id = ViewBag.MissionId }) %>');
  ko.applyBindings(model);
});
</script>
</asp:Content>