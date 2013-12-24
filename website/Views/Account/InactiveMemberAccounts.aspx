<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content" ContentPlaceHolderID="MainContent" runat="server">
  <h2>Inactive Member Accounts</h2>
  <p>
    The following members have accounts, but are no longer active with any unit.
  </p>
  <table class="data-table">
    <thead>
      <tr>
        <th>
          <input type="checkbox" data-bind="checked: SelectAll" /></th>
        <th>Last Name</th>
        <th>First Name</th>
        <th>Username</th>
        <th></th>
      </tr>
    </thead>
    <tbody data-bind="foreach: Rows">
      <tr>
        <td>
          <input type="checkbox" data-bind="value: Username, checked: $parent.Selected" /></td>
        <td><a data-bind="text: FirstName, attr: {href: '<%= Url.Action("Detail", "Members") %>/' + Id}"></a></td>
        <td data-bind="text: LastName"></td>
        <td data-bind="text: Username"></td>
        <td><button data-bind="click: $parent.DisableOne, disable: $root.Working">Disable</button></td>
      </tr>
    </tbody>
  </table>
  <div data-bind="visible: Working" style="display: none; font-weight:bold">Loading ...</div>
  <div data-bind="visible: Rows().length == 0 && !Working()" style="display: none">No inactive users with accounts.</div>
  <button data-bind="click: DisableSelected, enable: !Working() && Selected().length > 0">Disable Selected</button>

  <script type="text/javascript">
    var PageModel = function () {
      var self = this;
      this.Rows = ko.observableArray();
      this.Selected = ko.observableArray();
      this.Working = ko.observable(false);

      this.Load = function () {
        self.Working(true);
        self.Selected([]);
        $.ajax({ type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Account", action = "GetInactiveAccounts" }) %>', dataType: 'json', contentType: 'application/json; charset=utf-8' })
            .done(function (result) {
              self.Rows(result);
            })
        .always(function() {
          self.Working(false);
        });
      }

      self.SelectAll = ko.computed({
        read: function() {
          return (self.Selected().length == self.Rows().length) && self.Rows().length > 0;        
        },
        write: function(value) {
          self.Selected([]);
          if (value) {
            ko.utils.arrayForEach(self.Rows(), function (person) {
              self.Selected.push(person.Username);
            });
          }
        }
      });

      this.DisableOne = function (model) {
        return $.ajax({ type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Account", action = "DisableAccounts" }) %>', dataType: 'json', contentType: 'application/json; charset=utf-8',
          data: ko.toJSON([model.Username])})
        .always(function() {
          self.Load();
        });
      }

      this.DisableSelected = function () {
        return $.ajax({ type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Account", action = "DisableAccounts" }) %>', dataType: 'json', contentType: 'application/json; charset=utf-8',
          data: ko.toJSON(self.Selected())})
        .always(function() {
          self.Load();
        });
      }
    };

    NProgress.configure({ showSpinner: false });
    $(document).ajaxStart(function() { NProgress.start(); }).ajaxStop(function() { NProgress.done(); });

    $(document).ready(function () {
      var model = new PageModel();
      ko.applyBindings(model);
      model.Load();
    });
  </script>
</asp:Content>
