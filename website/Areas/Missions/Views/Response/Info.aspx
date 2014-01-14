<%@ Page Title="" Language="C#" MasterPageFile="~/Areas/Missions/Views/Response/Response.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <div class="container">
    <h4 data-bind="text:Mission.Title"></h4>
  </div><!-- /.container -->
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
<script type="text/javascript">
  var MissionInfoModel = function (missionInit) {
    var self = this;
    this.Title = ko.observable(missionInit.Title);
  }

  var PageModel = function (missionInit) {
    var self = this;
    this.Mission = new MissionInfoModel(missionInit);
  };

  $(document).ready(function () {
    var model = new PageModel(<%= Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.Data) %>);
    ko.applyBindings(model);
  });
</script>
</asp:Content>