<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<Guid>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<% bool canEdit = Page.User.IsInRole("cdb.trainingeditors"); %>


<div data-bind="allowBindings: false">
    <div id="trainingRecords">
        <table class="data-table" id="records_table">
            <thead>
                <tr>
                    <th>Course</th>
                    <th>Via</th>
                    <th>Completed</th>
                    <th>Expires</th>
                </tr>
                <!-- ko if: IsLoadingRecords -->
                <tr><td colspan="4"><img src="<%= Url.Content("~/content/images/progress.gif") %>" alt="Loading Documents ..." style="width:100%; height:12px" /></td></tr>
                <!-- /ko -->
            </thead>
            <tbody data-bind="foreach: List">
                <tr data-bind="css: { 'odd': ($index() % 2 > 0) }">
                    <td><a data-bind="text: Course.Title, attr: {href: '<%= Url.Action("Current", "Training") %>/'+Course.Id() }"></a></td>
                    <td>
                        <span data-bind="if: Source() == 'rule'">Rule</span>
                        <a data-bind="if: Source() == 'roster', attr: {href: '<%= Url.Action("Roster", "Training") %>/' + ReferenceId() }">Roster</a>
                        <a data-bind="if: Source() == 'direct', attr: {href: '<%= Url.Action("AwardDetails", "Training") %>/' + ReferenceId() }">Details</a>
                        <% if (canEdit)
                           {%>
                        <a data-bind="if: Source() == 'direct', attr: {href: '#' }, click: function() { $parent.EditRecord($data) }">Edit</a>
                        <a data-bind="if: Source() == 'direct', attr: {href: '#' }, click: function() { $parent.DeleteRecord($data) }">Delete</a>
                        <% } %>
                    </td>
                    <td data-bind="text: Completed"></td>
                    <td data-bind="text: Expires "></td>
                </tr>
            </tbody>
        </table>
        <div id="editRecordDialog" title="Update Training Record" style="display: none;" data-bind="jqEditDialog: RecordEditor">
            <div id="editRecordProcessing" style="display:none;" data-bind="text: RecordEditor.IsProcessing"></div>
            <p id="editRecordError" class="validateTips" data-bind="text: ifDefinedKO(RecordEditor.Errors(), '_root'), css: {'ui-state-error': isPropertyDefined(RecordEditor.Errors(), '_root')}" style="padding-left: 1em"></p>
            <form action="#" data-bind="with: RecordEditor.Model">
                <table style="width: 95%">
                    <tr>
                        <td style="width: 45%; border:none"><strong>Details:</strong>
                            <div style="border-top: solid 1px #888; padding-top:.5em">

                                <select id="recordcourse" name="Course" class="ui-corner-all" data-bind="options: filterCourses(Course.Id()), optionsValue: 'Id', optionsText: 'Title', value: Course.Id">
                                </select>

                                <label for="recordcompleted">Completed:</label>
                                <input class="datepicker" id="recordcompleted" name="Completed" type="text" data-bind="value: Completed" />

                                <p>
                                    <label for="recordexpiry">Expires:</label>
                                    <input id="recordexpirysrc1" name="Expirysrc" type="radio" value="default" data-bind="checked: ExpirySrc" />
                                    Use Course Default.<br />
                                    <input id="recordexpirysrc2" name="Expirysrc" type="radio" value="custom" data-bind="checked: ExpirySrc, event: { change: function() { setDatePickerEnabled('#recordexpiry', ExpirySrc() == 'custom'); } }" />
                                    Custom:
        <input class="datepicker" id="recordexpiry" type="text" data-bind="enable: ExpirySrc() == 'custom', value: Expires" /><br />
                                    <input id="recordexpirysrc3" name="Expirysrc" type="radio" value="never" data-bind="checked: ExpirySrc" />
                                    Never Expires (overrides course default).
                                </p>
                                <%--                        <p>ExpirySrc: <span data-bind="text: ExpirySrc">{null}</span></p>
                        <p>Expires: <span data-bind="text: Expires">{null}</span></p>
                        <p>Course ID: <span data-bind="text: Course.Id">{null}</span></p>
                                --%>
                            </div>
                        </td>
                        <td style="border:none"><strong>Comments:</strong>
                            <div style="border-top: solid 1px #888; padding-top:.5em">
                                <textarea class="ui-corner-all" cols="35" id="recordcomments" rows="5" data-bind="value: Comments"></textarea>
                            </div>
                        </td>
                    </tr>
                </table>
            </form>
            <div data-bind="with: RecordEditor">
                <strong>Documentation:</strong>
                <div style="border-top: solid 1px #888; padding: .5em;">
                    <div id="recordDocs" data-bind="jqFileUpload: $data">
                        <form id="fileupload" data-bind="attr: {action: '<%= Url.RouteUrl("DefaultApi", new { httproute = "", controller = "TrainingDocuments", action = "PutFiles" }) %>/' + Model().ReferenceId() }" method="POST" enctype="multipart/form-data">
                            <!-- ko if: IsLoadingDocs -->
                            <img src="<%= Url.Content("~/content/images/progress.gif") %>" alt="Loading Documents ..." style="width:100%; height:12px" />
                            <!-- /ko -->
                            <table style="font-size:90%">
                                <tbody>
                                    <!-- ko foreach: SavedDocuments -->
                                    <tr>
                                        <td><a data-bind="if: Thumbnail, attr: { href: Url, title: Title, download: Title }" rel="gallery">
                                            <img data-bind="attr: { src: Thumbnail + '?maxSize=24' }" /></a></td>
                                        <td><a data-bind="attr: {href: Url, title: Title, rel: Thumbnail , download: Title}, text: Title"></a></td>
                                        <td class="r" data-bind="text: formatFileSize(Size)"></td>
                                        <td>
                                            <img src="<%= Url.Content("~/content/images/trash.png") %>" alt="remove" style="width:20px;" data-bind="visible: !$parent.IsDeletingDoc(), click: function () { $parent.RemoveSaved($data) }" />
                                            <img src="<%= Url.Content("~/content/images/progress-sm.gif") %>" alt="removing..." style="width:20px;" data-bind="visible: $parent.IsDeletingDoc()" />
                                        </td>
                                    </tr>
                                    <!-- /ko -->
                                    <!-- ko foreach: PendingDocuments -->
                                    <tr style="background-color: #FFFFCC">
                                        <td>
                                            <div><img src="<%= Url.Content("~/Content/images/upload.gif") %>" alt="upload" /></td>
                                        <td data-bind="text: name"></td>
                                        <td class="r" data-bind="text: ($data['size'] == undefined) ? '' : formatFileSize(size)"></td>
                                        <td><img src="<%= Url.Content("~/content/images/trash.png") %>" style="width:20px;" alt="remove" data-bind="click: function() { $parent.RemovePending($data) }" /></td>
                                    </tr>
                                    <!-- /ko -->
                                </tbody>
                            </table>
                            <div class="fileupload-buttonbar" style="margin-top: .5em;">
                                <!-- The fileinput-button span is used to style the file input field as button -->
                                <span class="btn btn-success fileinput-button">
                                    <i class="icon-plus icon-white"></i>
                                    <span><% if (Request.Browser.Browser != "IE")
                                             { %>Drag and drop files<br />
                                        or<br />
                                        <% } %>Click to add ...</span>
                                    <input type="file" name="files[]" multiple="multiple" />
                                </span>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <% if (canEdit)
           { %>
        <button data-bind="click: AddRecord" id="addrecord">Add new record</button>
        <% } %>
    </div>
    </div>
    <script type="text/javascript">
        var globalCourseList = ko.observableArray();
        $.getJSON("<%= Url.RouteUrl("DefaultApi", new { httproute = "", controller = "TrainingCourses", action = "GetAll" }) %>", function (data) {
            globalCourseList.removeAll();
            for (var d in data) {
                globalCourseList.push(ko.mapping.fromJS(data[d]));
            }
        });

        function filterCourses(selectedId) {
            return ko.utils.arrayFilter(globalCourseList(), function (f) {
                return f.Id() == selectedId || f.Offered() == true;
            });
        }

        var TrainingRecordListModel = function () {
            var me = this;
            var recordMapping = buildKOMapping(TrainingAwardView);
            this.List = ko.mapping.fromJS([], recordMapping);
            var recordEditor = new ModelEditor(700, 480, function () {
                var newModel = new TrainingAwardView();
                newModel.Member.Id('<%= Model %>');
                var mapped = ko.mapping.fromJS(newModel, recordMapping);
                return mapped;
            });

            this.IsLoadingRecords = ko.observable(false);

            this.RecordEditor = recordEditor;
            recordEditor.SavedDocuments = ko.observableArray();
            recordEditor.PendingDocuments = ko.observableArray();
            recordEditor.IsLoadingDocs = ko.observable(false);
            recordEditor.IsDeletingDoc = ko.observable(false);
            recordEditor.IsSendingRecord = ko.observable(false);
            recordEditor.IsProcessing = ko.computed(function () { return recordEditor.IsLoadingDocs() || recordEditor.IsDeletingDoc() || recordEditor.IsSendingRecord() });

            recordEditor.RemoveSaved = function (model) {
                if (confirm('Removing "' + model.Title + '"\n\nThis action is immediate and can not be undone. Continue?')) {
                    recordEditor.IsDeletingDoc(true);
                    $.ajax("<%= Url.RouteUrl("DefaultApi", new { httproute = "", controller = "TrainingDocuments", action = "delete" }) %>/" + model.Id,
                        {
                            type: 'POST',
                            contentType: 'application/json',
                            dataType: 'json'
                        })
                    .done(function () { recordEditor.SavedDocuments.remove(model); })
                    .fail(function () { alert('Failed to remove document'); })
                    .always(function() { recordEditor.IsDeletingDoc(false); });
                }
                
            }
            recordEditor.RemovePending = function (model) {
                recordEditor.PendingDocuments.remove(model);
            }

            recordEditor.TryCommit = function (model) {
            var deferred = $.Deferred();

            var result = false;
            model.PendingUploads(recordEditor.PendingDocuments().length);
            recordEditor.IsSendingRecord(true);
            $.ajax("<%= Url.RouteUrl("DefaultApi", new { httproute = "", controller = "TrainingRecords", action = "Post" }) %>",
           {
               type: 'POST',
               contentType: "application/json",
               data: ko.mapping.toJSON(model),
               dataType: 'json'
           })
            .done(function (data) {
                // Successfully saved the training record. Now submit the documents to support it.
                recordEditor.Model().ReferenceId(data.ReferenceId);
                var list = recordEditor.PendingDocuments();

                // If there aren't any documents to upload, we're done.
                if (list.length == 0) deferred.resolve(model);
                // Otherwise, start uploading...
                $.each(list, function (d) {
                    var doc = list[d];
                    var promise = doc.dataContainer.data('data').submit();
                    promise
                        .done(function (f, status, response) {
                            // Successfully submitted one of the documents
                            var test = doc;
                            recordEditor.PendingDocuments.remove(doc);
                            $.each(f, function (idx) { recordEditor.SavedDocuments.push(f[idx]) });
                            // last one finished gets to signal completion
                            if (list.length == 0) deferred.resolve(model);
                        })
                        .fail(function () { alert('upload fail'); });
                    //.done(function (f) { me.PendingDocuments.remove(f); me.SavedDocuments.push(f); });
                });
            })
            .fail(function (jhr) {
                if (jhr.status == 400) me.RecordEditor.Errors(ko.mapping.fromJSON(jhr.responseText));
                deferred.reject();
            })
            .always(function () { me.RecordEditor.IsSendingRecord(false); });

            return deferred.promise();
        }

            this._editRecord = function (model) {
                me.RecordEditor.SavedDocuments.removeAll();
                me.RecordEditor.PendingDocuments.removeAll();
                me.RecordEditor.Errors([]);
                if (model != null) {
                    recordEditor.IsLoadingDocs(true);
                    $.ajax('<%= Url.RouteUrl("DefaultApi", new { httproute = "", controller = "TrainingDocuments", action = "GetList" }) %>/' + model.ReferenceId(),
               {
                   type: 'GET',
                   contentType: "application/json",
                   dataType: 'json',
                   success: function (data) {
                       me.RecordEditor.SavedDocuments(data);
                   },
                   statusCode: {
                       400: function (jhr, status, error) { me.RecordEditor.Errors(ko.mapping.fromJSON(jhr.responseText)); }
                   },
                   complete: function () { recordEditor.IsLoadingDocs(false); }
               });
                }
            me.RecordEditor.StartEdit(model).done(function (m) {
                me.ReloadHistory();
                if (isDefined('wacForm')) wacForm.ReloadData();
            });
        }

        this.AddRecord = function () {
            me._editRecord(null);
        }

        this.EditRecord = function (model) {
            me._editRecord(model);
        }

        this.DeleteRecord = function (model) {
            if (confirm('Delete "' + model.Course.Title() + '" on ' + model.Completed() + '?'))
            {
                $.ajax("<%= Url.RouteUrl("DefaultApi", new { httproute = "", controller = "TrainingRecords", action = "Delete" }) %>/" + model.ReferenceId(),
                {
                   type: 'POST',
                   contentType: "application/json"
                })
                .done(function () { me.ReloadHistory(); })
                .fail(function () { alert('Failed to delete record'); });
            }
        }

        this.ReloadHistory = function () {
            me.IsLoadingRecords(true);
            $.getJSON("<%= Url.RouteUrl("DefaultApi", new { httproute = "", controller = "TrainingRecords", action = ((ViewData["history"] != null) ? "FindForMember" : "FindComputedForMember"), id = Model }) %>")
                .done(function (data) {
                    ko.mapping.fromJS(data, me.List);
                })
                .complete(function () { me.IsLoadingRecords(false); });
        }
    }

    $(document).ready(function () {
        var model = new TrainingRecordListModel();
        ko.applyBindings(model, $('#trainingRecords')[0]);
        model.ReloadHistory();
    });
    </script>
