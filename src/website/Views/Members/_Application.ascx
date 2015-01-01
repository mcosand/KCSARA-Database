<%@ Control Language="C#" Inherits="ViewUserControl<Member>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.api.Models" %>
<div id="memberApplications">
<h2>Current Application:</h2>
    <table class="data-table">
        <thead>
          <tr>
            <th>Unit</th>
            <th>Status</th>
            <th>Started</th>
            <% if ((bool)ViewData["CanEditMember"]) Response.Write("<th class='noprint'></th>"); %>
          </tr>
        </thead>
        <tbody data-bind="foreach: Rows">
            <tr><td data-bind="text: Unit.Name"></td>
                <td data-bind="text: IsActive ? 'Active' : 'Inactive'"></td>
                <td data-bind="text: formatDateTime('yy-mm-dd', Started)"></td>
               <% if ((bool)ViewData["CanEditMember"]) { %><td><a href="#" data-bind="click: $root.doWithdraw">Withdraw</a></td><% } %>
            </tr>
        </tbody>
        <tfoot>
            <tr data-bind="visible: isLoading"><th colspan="5">Loading ...</th></tr>
        </tfoot>
    </table>
    <% if ((bool)ViewData["IsSelf"]) { %>
    <ul>
        <li><strong>Add your address</strong></li>
        <li><strong>Add at least one phone number</strong></li>
        <li><strong>Add at least one emergency contact number</strong></li>
    </ul>
    <% } %>

    <h4>Application Documents</h4>
    <table class="data-table">
        <thead>
            <tr>
                <th>Unit</th><th>Title</th><th>Required</th><th>Status</th><th>Date</th><th data-bind="visible: Documents.anyActions">Actions</th>
            </tr>
        </thead>
        <tbody data-bind="foreach: Documents">
            <tr><td data-bind="text: Unit.Name"></td>
                <td><a data-bind="text: Title, attr: { href: Url }"></a></td>
                <td data-bind="text: Required ? 'Yes' : 'No'"></td>
                <td data-bind="text: Status"></td>
                <td data-bind="text: formatDateTime('yy-mm-dd', StatusDate)"></td>
                <td data-bind="visible: $parent.Documents.anyActions">
                    <button class="link" data-bind="visible: (Actions & <%= (int)MemberUnitDocumentActions.UserReview %>), disable: $parent.Documents.isLoading, click: $parent.Documents.doDocReview">Review</button>
                    <button class="link" data-bind="visible: Actions & <%= (int)MemberUnitDocumentActions.Submit %>, disable: $parent.Documents.isLoading, click: $parent.Documents.doDocSubmitted">Submit</button>
                    <button class="link" data-bind="visible: (Actions & <%= (int)MemberUnitDocumentActions.Reject %>), disable: $parent.Documents.isLoading, click: $parent.Documents.doDocReject">Reject</button>
                    <button class="link" data-bind="visible: Actions & <%= (int)MemberUnitDocumentActions.Complete %>, disable: $parent.Documents.isLoading, click: $parent.Documents.doDocSignoff">Sign off</button>
                </td>
            </tr>
        </tbody>
        <tfoot>
            <tr data-bind="visible: Documents.isLoading"><th colspan="6">Loading ...</th></tr>
        </tfoot>
    </table>
</div>
<script type="text/javascript">
    var ApplicationsModel = function () {
        var self = this;
        this.isLoading = ko.observable(false);

        this.Rows = ko.observableArray([]);
        this.Documents = ko.observableArray([]);
        this.Documents.isLoading = ko.observable(true);
        this.Documents.setState = function (prompt, url) {
            var doit = confirm(prompt);
            if (doit) {
                self.Documents.isLoading(true);
                $.ajax({ type: 'POST', url: url, dataType: 'json', contentType: 'application/json; charset=utf-8' })
                .done(function (result) {
                    window.setTimeout(self.GetDocuments, 0);
                })
                .fail(getApiFailureHandler(self.Documents.isLoading));
            }
        };
        this.Documents.doDocReview = function (doc) {
            var mDocId = (doc.Id) ? "&mDocId=" + doc.Id : "";
            self.Documents.setState(
                "Do you want to record that you've reviewed and understand the contents of the document '" + doc.Title + "'?",
                '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Members", action = "SignUnitDocument", id=Model.Id }) %>/?docId=' + doc.DocId + mDocId
                );};

        this.Documents.doDocSubmitted = function (doc) {
            var mDocId = (doc.Id) ? "&mDocId=" + doc.Id : "";
            self.Documents.setState(
            "Press 'OK' to indicate that you have submitted the completed document '" + doc.Title + "' to:\n\n" + doc.SubmitTo,
            '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Members", action = "SubmitUnitDocument", id = Model.Id }) %>/?docId=' + doc.DocId + mDocId
            );};

        this.Documents.doDocSignoffEx = function (doc, approve) {
            var mDocId = (doc.Id) ? "&mDocId=" + doc.Id : "";

            self.Documents.setState(
                    (approve ? "Accept" : "Reject") + " document '" + doc.Title + "'?",
                    '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Members", action = "SignoffUnitDocument", id=Model.Id }) %>/?docId=' + doc.DocId + mDocId + "&approve=" + approve
            );
        };

        this.Documents.doDocSignoff = function (doc) { self.Documents.doDocSignoffEx(doc, true); };
        this.Documents.doDocReject = function (doc) { self.Documents.doDocSignoffEx(doc, false); };

        this.Documents.anyActions = ko.computed(function () {
            var list = this.Documents();
            for (var i = 0; i < list.length; i++) {
                if (list[i].Actions > 0) return true;
            }
            return false;
        }, this);

        this.GetData = function ()
        {
            this.isLoading(true);
            $.ajax({ type: 'GET', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Members", action = "GetApplications", id=Model.Id }) %>', dataType: 'json', contentType: 'application/json; charset=utf-8' })
            .done(function (result) {
                self.Rows(makeDates(result, ['Started']));
                window.setTimeout(self.GetDocuments, 0);
            })
            .always(function () {
                self.isLoading(false);
            });
        }

        this.GetDocuments = function () {
            self.Documents.isLoading(true);
            $.ajax({ type: 'GET', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Members", action = "GetUnitDocuments", id=Model.Id }) %>', dataType: 'json', contentType: 'application/json; charset=utf-8' })
            .done(function (result) {
                self.Documents(makeDates(result, ['StatusDate']));
            })
            .always(function () {
                self.Documents.isLoading(false);
            });
        }

        this.doWithdraw = function (row) {
            var msg = 'Withdraw application?';
            if (self.Rows.length == 1) msg += "\nAll records associated with the account will be removed.";

            var doit = confirm(msg);
            if (doit) {
                $.ajax({ type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Units", action = "WithdrawApplication" }) %>/' + row.Id, dataType: 'json', contentType: 'application/json; charset=utf-8' })
                .done(function (result) {
                    self.GetData();
                })
                .fail(getApiFailureHandler(null));
            }
        }
    };

    $(document).ready(function () {
        var model = new ApplicationsModel();
        ko.applyBindings(model, $('#memberApplications')[0]);
        model.GetData();
    });
</script>