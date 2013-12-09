<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<SarUnit>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsar.Database.Model" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  <h2><%: Model.DisplayName %> Applicants</h2>
  
  <table id="t">
    <thead>
      <tr><th></th><th>Name</th><th>Email</th><th>Status</th><th>Submit Date</th><th>Background</th><th>Username</th><th>Emer. Con.</th><th>Docs Left</th></tr>
    </thead>
    <tbody data-bind="foreach:Rows">
        <tr>
            <td><input type="checkbox" data-bind="value: Id, checked: $parent.selected" /></td>
            <td><a data-bind="attr: { href: '<%= Url.Action("Detail", "Members") %>/' + MemberId }, text: NameReverse"></a></td>
            <td data-bind="text:Email"></td>
            <td data-bind="text: Active ? 'Active' : 'Inactive'"></td>
            <td data-bind="text: formatDateTime('yy-mm-dd', Started)"></td>
            <td data-bind="text: Background"></td>
            <td data-bind="text: Username"></td>
            <td data-bind="text: EmergencyContactCount"></td>
            <td data-bind="text: RemainingDocCount"></td>
<%--          <td><input type="checkbox" onchange="model.setSelected('<%= a.Applicant.Id %>', this.checked);"/></td>
          <td><%= Html.ActionLink<MembersController>(x => x.Detail(a.Applicant.Id), a.Applicant.ReverseName) %></td>
          <td><%: a.Applicant.ContactNumbers.OrderBy(f => f.Priority).Where(f => f.Type == "email").Select(f => f.Value).FirstOrDefault() %></td>
          <td><%: a.IsActive ? "Active" : "Inactive" %></td>
          <td><%: string.Format("{0:yyyy-MM-dd}", a.Started) %></td>
          <td><%: a.Applicant.BackgroundText %></td>
          <td><%: a.Applicant.Username %></td>
          <td><%: a.Applicant.EmergencyContacts.Count %></td>--%>
        </tr>
    </tbody>
  </table>
    <div data-bind="text: ko.toJSON($data)"></div>
  <script>
      

      var PageModel = function () {
          var self = this;

          this.Rows = ko.observableArray();

          this.selected = ko.observableArray();

          this.isLoading = ko.observable(true);
          this.GetData = function () {
              this.isLoading(true);
              $.ajax({ type: 'GET', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Units", action = "GetApplicants", id=Model.Id }) %>', dataType: 'json', contentType: 'application/json; charset=utf-8' })
            .done(function (result) {
                self.Rows(makeDates(result, ['Started']));
            })
            .always(function () {
                self.isLoading(false);
            });
        }

      }

      var model = new PageModel();
      $(document).ready(function () {
          model.GetData();
          ko.applyBindings(model);
    //$("#t").tablesorter({ widgets: ['zebra'] });
  });
</script>
</asp:Content>
  