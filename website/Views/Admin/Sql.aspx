<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <h2>Execute SQL Database Script</h2>
  <label for="store">Database:</label>
  <select id="store" data-bind="value:Store">
    <option value="DataStore">Data Storage</option>
    <% if (ViewBag.IsAccountAdmin)
       { %><option value="AuthStore">Permissions</option>
    <% } %>
  </select>
  <label for="updateKey">Update Key:</label><input type="text" id="updateKey" data-bind="value: Key" style="width:20em" />
  <label for="sql">SQL Text:</label>
  <textarea rows="20" cols="70" id="sql" data-bind="value: Sql"></textarea>
  <div><button data-bind="enable:!Working() && !Submitted(), click: Submit">Submit</button> <button data-bind="enable:Submitted() && !Working(), click:Unlock">Unlock</button></div>
  <pre id="result" data-bind="text: Result"></pre>
<script type="text/javascript">
  var PageModel = function () {
    var self = this;
    this.Store = ko.observable();
    this.Key = ko.observable();
    this.Sql = ko.observable();
    this.Result = ko.observable();
    this.Working = ko.observable(false);
    this.Submitted = ko.observable(false);
    this.Submit = function () {
      self.Submitted(true);
      self.Working(true);
      self.Result("");
      $.ajax({
        type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Admin", action = "Sql" }) %>', dataType: 'json', contentType: 'application/json; charset=utf-8',
        // data: { sql: self.Sql(), updateKey: self.Key(), store: self.Store() }
        data: ko.toJSON({ sql: self.Sql(), updateKey: self.Key(), store: self.Store() })
      })
        .done(function(result) { self.Result(result); })
        .always(function () { self.Working(false); });
    }

    this.Unlock = function()
    {
      self.Submitted(false);
    }
  }

  $(document).ready(function () {
    var model = new PageModel();
    ko.applyBindings(model);
  });

</script>
</asp:Content>
