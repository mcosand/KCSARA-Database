<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<SarUnit>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% SarUnit u = ViewData.Model; %>

<h2>Details for unit: <%= u.DisplayName %></h2>
  <div>
    <%= Html.ActionLink<UnitsController>(x => x.Roster(u.Id), "View Roster") %><br />
    <%= Html.ActionLink<UnitsController>(x => x.DownloadRoster(u.Id, null), "Download Roster")%> (Contact information for active members)<br />
    <%= Html.ActionLink<UnitsController>(x => x.MissionReadyList(u.Id), "Download Mission Ready List")%><br />
    <%= Html.ActionLink<TrainingController>(x => x.CoreCompReport(u.Id), "Download Core Competency Report")%><br />
  </div>
    <div>
        <h3>Applications</h3>
        <% if ((bool)ViewBag.CanApply)  { %>
        <p><%: u.DisplayName %> is currently taking applications.</p>
            <button data-bind="jqButtonEnable: applyEnabled, click: doApply">Apply Now</button>
        <% } else if (!string.IsNullOrWhiteSpace(ViewBag.NoApplyReason)) { %>
        <p><%: u.DisplayName %> is not currently taking applications. &quot;<%: ViewBag.NoApplyReason %>&quot;</p>
        <% } %>
    </div>
  <div>
    <h3>Unit Status Types</h3>
    <table>
      <tr>
        <th>Status Name</th>
        <th>Is Active?</th>
        <th>WAC Level</th>
        <% if (Page.User.IsInRole("cdb.admins")) Response.Write("<th></th>"); %>
      </tr>
    <% foreach (UnitStatus s in u.StatusTypes.OrderBy(x => x.StatusName)) { %>
      <tr>
        <td><%= s.StatusName %></td>
        <td><%= s.IsActive %></td>
        <td><%= s.WacLevel %></td>
        <% if (Page.User.IsInRole("cdb.admins")) { %>
        <td>
          <%= Html.PopupActionLink<UnitsController>(x => x.EditStatus(s.Id), "Edit", 260)%>
          <%= Html.PopupActionLink<UnitsController>(x => x.DeleteStatus(s.Id), "Delete", 200)%></td>
          <% } %>
      </tr>    
    <% } %>
    </table>
    <% if (Page.User.IsInRole("cdb.admins")) { %>
    <%= Html.PopupActionLink<UnitsController>(x => x.CreateStatus(u.Id), "Add Unit Status Type", 260)%>
    <% } %>
  </div>
    <% if (ViewBag.CanEditDocuments) { %>
    <div>
        <h3>Unit Documents</h3>
        <table class="data-table">
            <thead>
              <tr><th>Title</th><th>Required</th><th>Download URL</th><th>Submit To</th><th>Display<br />Order</th><th>For Members<br />Older Than</th><th>For Members<br />Younger Than</th><th></th></tr>
            </thead>
            <tbody data-bind="foreach: documents">
                <tr>
                    <td><input type="text" data-bind="value: Title" /></td>
                    <td><input type="checkbox" data-bind="checked: Required" /></td>
                    <td><input type="text" data-bind="value: Url" /></td>
                    <td><textarea data-bind="value: SubmitTo"></textarea></td>
                    <td><input type="text" data-bind="value: Order" style="width:2em" /></td>
                    <td><input type="text" data-bind="value: ForMembersOlder" style="width:2em; display:inline" /> yrs</td>
                    <td><input type="text" data-bind="value: ForMembersYounger" style="width:2em; display:inline" />&nbsp;yrs</td>
                    <td style="white-space:nowrap"><button class="link" data-bind="click: $parent.documents.doDelete"><%= Strings.ActionDelete %></button></td>
                </tr>
            </tbody>
            <tfoot data-bind="visible: documents.isLoading">
                <tr><th colspan="8">Loading ...</th></tr>
            </tfoot>
        </table>
        <button data-bind="jqButtonEnable: !documents.isLoading(), click: documents.doAdd">Add Document</button>
        <button data-bind="jqButtonEnable: !documents.isLoading(), click: documents.doSave">Save Changes</button>
    </div>
    <% } %>
    <script type="text/javascript">
        var UnitDocument = function (model) {
            this.Id = ko.observable(model.Id);
            this.Title = ko.observable(model.Title);
            this.Required = ko.observable(model ? model.Required : true);
            this.Url = ko.observable(model.Url);
            this.SubmitTo = ko.observable(model.SubmitTo);
            this.Order = ko.observable(model.Order);
            this.ForMembersYounger = ko.observable(model.ForMembersYounger);
            this.ForMembersOlder = ko.observable(model.ForMembersOlder);
        }

        var PageModel = function () {
            var self = this;
            this.applyEnabled = ko.observable(true);

            this.doApply = function () {
                var doit = confirm("Apply to <%: u.DisplayName %>?");
                if (doit) {
                    self.applyEnabled(false);

                    $.ajax({ type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Units", action = "SubmitApplication", id=u.Id }) %>?memberId=<%: (Guid)ViewBag.ActorId %>', dataType: 'json', contentType: 'application/json; charset=utf-8' })
                    .done(function (result) {
                        window.location.href = '<%= Url.Action("Detail", "Members", new { id=ViewBag.ActorId }) %>';
                    })
                    .fail(function (err) {
                        alert(err.responseJSON.Message);
                        self.applyEnabled(true);
                    })
                }
            }

            this.documents = ko.observableArray();
            this.documents.isLoading = ko.observable(true);
            this.documents.getData = function () {
                self.documents.isLoading(true);
                $.ajax({ type: 'GET', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Units", action = "GetDocuments", id=Model.Id }) %>', dataType: 'json', contentType: 'application/json; charset=utf-8' })
                        .done(function (result) {
                            self.documents($.map(result, function (val) { return new UnitDocument(val); }));
                        })
                        .always(function () {
                            self.documents.isLoading(false);
                        });
                };

            this.documents.doAdd = function () {
                self.documents.push(new UnitDocument({Order: self.documents().length + 1}));
            }

            this.documents.doDelete = function (row) {
                self.documents.remove(row);
            }

            this.documents.doSave = function () {
                self.documents.isLoading(true);
                $.ajax({ type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Units", action = "SaveDocuments", id=Model.Id }) %>', data: ko.toJSON(self.documents), dataType: 'json', contentType: 'application/json; charset=utf-8' })
                    .done(function (result) {
                        if (result == 'OK') { alert('Saved successfully'); }
                        else { alert(result); }
                    })
                    .always(function () {
                        self.documents.isLoading(false);
                    });
            }

            this.load = function () {
                self.documents.getData();
            }
        }

        var model = new PageModel();
        model.load();
        $(document).ready(function () {
            $('#content').find('button').not("[class='link']").button();
            $('#content').find('input[type="text"]').addClass("input-box");
            $('#content').find('textarea').addClass("input-box");
            ko.applyBindings(model);
        });
    </script>
</asp:Content>
