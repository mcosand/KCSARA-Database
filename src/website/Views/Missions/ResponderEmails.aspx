<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Missions/Missions.Master" Inherits="System.Web.Mvc.ViewPage<Kcsar.Database.Model.Mission>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <% Html.RenderPartial("Mission", ViewData["mission"]); %>

    <h2>Responder Emails</h2>
    <fieldset>
        <label for="unitSelect">Filter to Unit:</label>
        <select id="unitSelect" data-bind="options: units, optionsText: 'T', optionsValue: 'V', value: unit"></select><br />
        <input id="useNewline" type="checkbox" data-bind="checked: useNewline" /><label for="useNewline">One address per line</label>
    </fieldset>
    <p data-bind="visible: isLoading">LOADING ...</p>
    <p data-bind="html: emailString"></p>
    <h3 data-bind="visible: noEmails().length">Responders Without Email:</h3>
    <div data-bind="foreach: noEmails, visible: noEmails().length">
        <p data-bind="text: FirstName + ' ' + LastName" style="margin-left: 2em"></p>
    </div>

    <script type="text/javascript">
        var PageModel = function (missionId) {
            var self = this;
            this.responders = ko.observableArray();
            this.unit = ko.observable();
            this.units = ko.observableArray();
            this.useNewline = ko.observable();

            this.isLoading = ko.observable(true);

            this.emails = ko.computed(function () {
                return $.grep(this.responders(), function (n, i) {
                    return n.Contacts[0] != null && (self.unit() == null || n.Units[0] == self.unit())
                });
            }, this);

            this.emailString = ko.computed(function () {
                return $.map(this.emails(), function (n, i) { return '"' + n.FirstName + ' ' + n.LastName + '" &lt;' + n.Contacts[0].Value + '&gt;' }).join(this.useNewline() ? "<br/>" : '; ')
            }, this);

            this.noEmails = ko.computed(function () {
                return $.grep(this.responders(), function (n, i) {
                    return n.Contacts[0] == null && (self.unit() == null || n.Units[0] == self.unit())
                });
            }, this);

            this.load = function () {
                self.isLoading(true);
                $.ajax({ type: 'GET', url: '<%= Url.Content("~/api/Missions/GetResponderEmails") %>/' + missionId + "?unitid=", data: null, dataType: 'json' })
            .done(function (data) {
                self.responders.removeAll();
                self.units.removeAll();
                unitDict = {}
                for (var i in data) {
                    self.responders.push(data[i]);
                    unitDict[data[i].Units[0]] = 1;
                }

                self.units.push({ T: 'All Units', V: null });
                for (var k in unitDict) self.units.push({ T: k, V: k });

            })
            .fail(function (msg) { alert('Could not load data'); })
            .done(function () { self.isLoading(false); });
        }
    }

    $(document).ready(function () {
        var model = new PageModel('<%= Model.Id %>');
        ko.applyBindings(model);
        model.load();
    });
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        label {
            display: inline;
            font-size: 90%;
        }
    </style>
</asp:Content>
