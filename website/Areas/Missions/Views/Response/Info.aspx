<%@ Page Title="" Language="C#" MasterPageFile="~/Areas/Missions/Views/Response/Response.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

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
    <form class="form-horizontal" role="form" data-bind="with:EditResponse">
      <div class="form-group">
        <label for="editUnit" class="col-sm-3 control-label">Unit</label>
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
      <input type="submit" class="btn-lg btn-block btn-primary" value="Respond" data-bind="click:$root.postResponse, disable:$root.IsWorking" />
     </form>
  <!-- /ko -->
    </div><!-- /.container -->
  <div class="container">

  </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
<%= Scripts.Render("~/Areas/Missions/Scripts/Response/InfoPage.js") %>
<script type="text/javascript">
  $(document).ready(function () {
    var model = new PageModel(<%= Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.Data) %>, <%= Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.MyUnits) %>);
  ko.applyBindings(model);
});
</script>
</asp:Content>