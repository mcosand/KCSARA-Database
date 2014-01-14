<%@ Page Title="" Language="C#" MasterPageFile="~/Areas/Missions/Views/Response/Response.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <div class="container" style="padding-top:10px">
    <% if (ViewBag.IsMissionEditor == null) { %><div class="alert alert-warning alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button><strong>You are not a mission editor!</strong> You are viewing this form
      because others may not currently have access to this site. Your actions will be logged, and careless use of this form may lead to disciplinary action.</div><% } %>
    <form class="form-horizontal" role="form" data-bind="with:Mission">
      <div class="form-group" data-bind="css:{'has-error': Title.invalid}">
        <label for="createTitle" class="col-sm-3 control-label">Mission Title</label>
        <div class="col-sm-9">
          <input type="text" id="createTitle" data-bind="value:Title, event: {blur:Title.validate}, hasFocus: Title.hasFocus" class="form-control" placeholder="Required"/>
          <div class="help-block" data-bind="foreach:Title.errors, visible:Title.invalid">
            <div class="text-danger" data-bind="text:$data"></div>
          </div>
        </div>
      </div>
      <div class="form-group" data-bind="css: { 'has-error': Location.invalid }">
        <label for="createLocation" class="col-sm-3 control-label">Staging Location</label>
        <div class="col-sm-9">
          <input type="text" id="createLocation" data-bind="value:Location, event: {blur:Location.validate}, hasFocus: Location.hasFocus" class="form-control" placeholder="Required"/>
          <div class="help-block" data-bind="foreach: Location.errors, visible: Location.invalid">
            <div class="text-danger" data-bind="text: $data"></div>
          </div>
        </div>
      </div>
      <div class="form-group" data-bind="css:{'has-error': Started.invalid}">
        <label for="createStarted" class="col-sm-3 control-label">Start Time</label>
        <div class="col-sm-9">
          <input type="text" id="createStarted" data-bind="value: Started.formatted, event: { blur: Started.validate }" class="form-control" />
          <div class="help-block" data-bind="foreach: Started.errors, visible: Started.invalid">
            <div class="text-danger" data-bind="text: $data"></div>
          </div>
        </div>
      </div>
      <input type="submit" class="btn-lg btn-block btn-primary" value="Create Mission" data-bind="click:$root.postMission, disable:$root.isWorking" />
    </form>
    <div data-bind="text:ko.toJSON($data.Mission)" />
  </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
  <script type="text/javascript">
    var PageModel = function () {
      var self = this;
      this.Mission = {};

      this.Mission.Title = ko.observable();
      this.Mission.Title.hasFocus = ko.observable(false);
      this.Mission.Title.errors = ko.observableArray([]);
      this.Mission.Title.invalid = ko.computed(function () { return this.Mission.Title.errors().length > 0; }, this);
      this.Mission.Title.validate = function () {
        self.Mission.Title.errors([]);
        if (self.Mission.Title() == '' || self.Mission.Title() == null) self.Mission.Title.errors.push('Required');
      }

      this.Mission.Location = ko.observable();
      this.Mission.Location.hasFocus = ko.observable(false);
      this.Mission.Location.errors = ko.observableArray([]);
      this.Mission.Location.visitedFlag = ko.observable(false);
      this.Mission.Location.invalid = ko.computed(function () { return this.Mission.Location.errors().length > 0 }, this);
      this.Mission.Location.validate = function () {
        self.Mission.Location.errors([]);
        if (self.Mission.Location() == '' || self.Mission.Location() == null) self.Mission.Location.errors.push('Required');
      }

      this.Mission.Started = createMomentObservable(true);
      

      this.Mission.no = ko.observable();
      this.Mission.no.valid = ko.computed(function () {
        return !(this.Mission.Title.invalid() || this.Mission.Location.invalid() || this.Mission.Started.invalid());
      }, this);
      
      this.ShowBack = ko.observable(self.cancelMission);
      //this.Menu = ko.observableArray([{ title: 'foo', target: '#blah' }, { title: 'bar', target: '#other' }]);
      this.cancelMission = function () {
        history.go(-1);
      }
      this.isWorking = ko.observable(false);

      this.postMission = function () {
        self.isWorking(true);
        $.ajax({
          type: 'POST', url: '<%=  Url.RouteUrl(Kcsara.Database.Web.Areas.Missions.api.ResponseApiController.RouteName, new { httproute="", controller = "ResponseApi", action = "Create" }) %>', data: ko.toJSON(self.Mission), dataType: 'json', contentType: 'application/json; charset=utf-8'
        } )
        .done(function (result) {
          window.location.href = '<%= Url.Action("Info") %>/' + result.MissionId;
        })
          .fail(function (error) {
            if (error.status == 400) {
              for (var key in error.responseJSON) {
                var key2 = key.replace(/.*\./, '');
                self.Mission[key2].errors(error.responseJSON[key]);
              }
            }
            else {
              alert(error.responseText);
            }
          })

        .always(function(errorMsg) {
          self.isWorking(false);
        })


      }
    };

    $(document).ready(function () {
      var model = new PageModel();
      model.Mission.Title.hasFocus(true);
      ko.applyBindings(model);
    });
  </script>
</asp:Content>
