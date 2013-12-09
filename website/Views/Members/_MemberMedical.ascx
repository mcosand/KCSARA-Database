<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<Member>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

    
    <div id="medInformation">
        <% if ((bool)ViewData["IsSelf"]) { %>
        <p style="font-size:75%">Your information is protected. <a href="#" data-bind="click: doPrivacy">Read more</a>.</p>
          <div id="medPrivacyDialog" data-bind="jqModalMessage: { autoOpen: false, title: 'KCSARA Privacy Notice', width:500, height:500 }, dialogVisible: showPrivacy">
              <p>The volunteers of KCSARA value your privacy and have taken steps to protect it. We continuously work with the Sheriff's office to determine the minimum amount of
                  sensitive information we need to collect in order to conduct our business of search and rescue. Providing this medical information is optional. KCSARA volunteers
                  are expected to work within their capabilities, self-limit, and make the staff aware of any medical issues that may affect your participation in a training or mission</p>
              <p>Here's how you can expect to have your information treated:</p>
              <ul>
                  <li>It is encrypted as it is sent over the internet to our server.</li>
                  <li>It is stored in encrypted form in our database.</li>
                  <li>An entry is logged every time a user (besides you) retrieves the information.</li>
                  <li>You may request a copy of the log showing dates and reasons for all data access.</li>
              </ul>
          </div>
        <% } %>
        <button data-bind="click: doOpen">Review</button>

        <div id="medInfoDialog" data-bind="jqModalMessage: { autoOpen: false, title: 'Medical Information', width:600, height:400 }, dialogVisible: showDialog">
            <div style="max-width:550px;position:relative">
                <div style="width:100%;text-align:center;margin:auto auto;padding:4em;z-index:5000;position:absolute;background:white" data-bind="visible:isLoading">Loading ...</div>

            <label for="medAllergy">Allergies</label>
            <input id="medAllergy" type="text" class="input-box" style="width:30em" data-bind="value: Data.Allergies, visible: canEdit" />
            <p data-bind="text: Data.Allergies, visible: !canEdit()"></p>

            <label for="medMeds">Regular Medications:</label>
            <input id="medMeds" type="text" class="input-box" style="width:30em" data-bind="value: Data.Medications, visible: canEdit" />
            <p data-bind="text: Data.Medications, visible: !canEdit()"></p>

            <label for="medDisclosure">Medical/Physical Conditions which may affect participation in SAR activites:</label>
            <textarea id="medDisclosure" class="input-box" style="width:30em; height:4em;" data-bind="value: Data.Disclosure, visible: canEdit"></textarea>
            <p data-bind="text: Data.Disclosure, visible: !canEdit()"></p>

            <label>Emergency Contacts:</label>
            <table class="data-table">
                <thead>
                    <tr><th style="width:10em">Name</th><th style="width:8em">Relation</th><th style="width:6em">Type</th><th style="width:10em">Number</th><th data-bind="visible:canEdit"></th></tr>
                </thead>
                <tbody data-bind="foreach: Data.Contacts">
                    <tr><td><input type="text" class="input-box" data-bind="value: Name, visible: $root.canEdit" /><span data-bind="text:Name, visible: !$root.canEdit()" /></td>
                        <td><input type="text" class="input-box" data-bind="value: Relation, visible: $root.canEdit" style="width:8em" /><span data-bind="text:Relation, visible: !$root.canEdit()" /></td>
                        <td><!-- ko if: $root.canEdit --><select class="input-box" data-bind="value: Type, visible: $root.canEdit"><option value="cell">Cell</option><option value="home">Home</option><option value="work">Work</option><option value="other">Other</option></select><!-- /ko -->
                            <span data-bind="text:Type, visible: !$root.canEdit()" /></td>
                        <td><input type="text" class="input-box" data-bind="value: Number, visible: $root.canEdit" /><span data-bind="text:Number, visible: !$root.canEdit()" /></td>
                        <td data-bind="visible: $root.canEdit"><a href="#" data-bind="click:$root.removeContact"><img src="<%= Url.Content("~/content/images/trash.png") %>" style="width:1.8em" alt="Delete" title="Delete" /></a></td></tr>
                </tbody>
            </table>
            <a href="#" data-bind="click:addContact, visible:canEdit" >Add Emergency Contact</a><br />
            <button data-bind="click:doSensitive, visible:hasSensitive() && !showingSensitive()">Show Details</button><br />
            <button data-bind="click:doSave, visible:canEdit">Save</button>
            </div>
        </div>
    </div>
<script type="text/javascript">
    var EmergencyContact = function () {
        this.Id = '00000000-0000-0000-0000-000000000000';
        this.Name = ko.observable();
        this.Relation = ko.observable();
        this.Type = ko.observable();
        this.Number = ko.observable();
    }

    var MedicalInformationPanel = function () {
        var self = this;
        this.isLoading = ko.observable(false);
        this.showDialog = ko.observable(false);
        this.showPrivacy = ko.observable(false);
        this.hasSensitive = ko.observable(false);
        this.sensitiveLoaded = ko.observable(false);

        this.showingSensitive = ko.computed(function () {
            return (this.hasSensitive() && this.sensitiveLoaded()) || !this.hasSensitive();
        }, this);
        this.canEdit = ko.computed(function () {
            return this.showingSensitive() && !this.isLoading() && <%= ((bool)ViewData["CanEditSelf"]).ToString().ToLowerInvariant() %>;
        }, this);

        this.Data = {
            Member: { Id: '<%: Model.Id %>' },
            Allergies: ko.observable(''),
            Medications: ko.observable(''),
            Disclosure: ko.observable(''),
            Contacts: ko.observableArray([]),
        };

        var loadData = function (sensitive, reason) {
            self.isLoading(true);
            var reasonArg = (reason == null) ? '' : "&reason=" + encodeURIComponent(reason);
            $.ajax({ type: 'GET', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Members", action = "GetMedical", id=Model.Id }) %>?showSensitive=' + sensitive + reasonArg, dataType: 'json', contentType: 'application/json; charset=utf-8' })
                        .done(function (result) {
                            self.hasSensitive(result.Allergies || result.Medications || result.Disclosure || result.Contacts.length > 0);
                            self.sensitiveLoaded(sensitive);
                            self.Data.Allergies(result.Allergies);
                            self.Data.Medications(result.Medications);
                            self.Data.Disclosure(result.Disclosure);
                            self.Data.Contacts(result.Contacts);
                        })
                        .fail(function() {
                            alert('Error getting data from server');
                            self.hasSensitive(true);
                        })
                        .always(function () {
                            self.isLoading(false);
                        });

        }

        this.doOpen = function() {
            self.showDialog(true);
            loadData(<%= ((bool)ViewData["IsSelf"]).ToString().ToLowerInvariant() + "," %> null);
        }

        this.doPrivacy = function() {
            self.showPrivacy(true);
        }

        this.doSensitive = function () {
            var reason = null; 
            <% if (!(bool)ViewData["IsSelf"]) { %>
            do
            {
              reason = prompt("This action will be logged. Please enter your reason for viewing this data.");
            } while (reason == "");
            if (reason == null) return;
            <% } %>
            loadData(true, reason);
        }

        this.addContact = function () {
            var c = new EmergencyContact();
            c.Type('cell');
            self.Data.Contacts.push(c);
        }

        this.removeContact = function(c)
        {
            self.Data.Contacts.remove(c);
        }

        this.doSave = function() {
            self.isLoading(true);
            $.ajax({ type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Members", action = "SaveMedical" }) %>', data: ko.toJSON(self.Data), dataType: 'json', contentType: 'application/json; charset=utf-8' })
            .done(function (result) {
                if (result == 'OK') { self.showDialog(false); }
                else { alert(result); }
            })
            .always(function () {
                self.isLoading(false);
            });
        }
    }

    $(document).ready(function () {
        var model = new MedicalInformationPanel();
        ko.applyBindings(model, $('#medInformation')[0]);
    });
</script>