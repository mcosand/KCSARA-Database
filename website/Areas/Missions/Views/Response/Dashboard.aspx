<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Core.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2>Mission Response Dashboard</h2>

  <table class="data-table">
    <thead>
      <tr><th>Mission</th><th>Started</th><th>Stage?</th><th>Call?</th></tr>
    </thead>
    <!-- ko ifnot: Missions.isLoading -->
    <tbody data-bind="foreach: Missions">
      <tr>
        <td data-bind="text: Title"></td>
        <td data-bind="text: Started.format('ddd MMM D')"></td>
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

<script type="text/javascript">
  var PageModel = function () {
    var self = this;
    this.Missions = ko.observableArray();
    this.Missions.isLoading = ko.observable(true);

    this.loadMissions = function () {
      self.Missions.isLoading(true);
      $.ajax({ type: 'GET', url: '<%= Url.Content("~/api/Missions/Response/GetCurrentStatus") %>', data: null, dataType: 'json' })
        .done(function (data) {
          for (i = 0; i < data.length; i++) mapMoment(data[i], ['Started', 'NextStart', 'StopStaging']);
          self.Missions(data);
        })
        .always(function () {
          self.Missions.isLoading(false);
        });
    };

    this.load = function () {
      self.loadMissions();
    }
  }

  $(document).ready(function () {
    var model = new PageModel();
    ko.applyBindings(model);
    model.load();
  });
</script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
