/*
 * Copyright 2012-2014 Matthew Cosand
 */
function buildKOMapping(viewType) {
    return {
        create: function (options) {
            var result = new viewType();
            result.fromJS(options.data);
            return result;
        },
        key: function (data) {
            return ko.utils.unwrapObservable(data.Id);
        }
    };
}

function TrainingAwardView() {
    var self = this;
    this.Course = {
        Id: ko.observable(),
        Title: ko.observable(),
        Required: ko.observable()
    };
    this.Member = {
        Id: ko.observable()
    };
    this.Completed = ko.observable(formatDateTime("yy-mm-dd", new Date()));
    this.Expires = ko.observable();
    this.ExpirySrc = ko.observable("default");
    this.Source = ko.observable();
    this.ReferenceId = ko.observable();
    this.Comments = ko.observable();
    this.Required = ko.observable();
    this.PendingUploads = ko.observable();
}
TrainingAwardView.prototype.fromJS = function (data) {
    ko.mapping.fromJS(data, {}, this);
    return this;
}

//=====================================================================================

ko.bindingHandlers.allowBindings = {
    init: function (elem, valueAccessor) {
        // Let bindings proceed as normal *only if* my value is false
        var shouldAllowBindings = ko.utils.unwrapObservable(valueAccessor());
        return { controlsDescendantBindings: !shouldAllowBindings };
    }
};


function ifDefinedKO(model, property) {
    if (model == null || model[property] == undefined) {
        return null;
    }
    return model[property]();
}


/* SAMPLE PAGE VIEW MODEL WITH A LIST OF OBJECTS, EACH ONE CAN BE EDITED IN A jQuery DIALOG

  var PageModel = function () {
    var me = this;
    this.ModelEditorA = new ModelEditor(700, 480, function () { return new ComplexObject(); });
    this.Records = ko.observableArray();
    this.EditTypeA = function (index) {
        // Keep track of the record we're editing
        var editing = me.Records()[index()];

        // Tell the editor to start doing its thing.
        // We use jQuery's Promise to copy back the edited object after successfully completing an edit
        me.ModelEditorA.StartEdit(editing).done(function (m) {
            me.Records.replace(editing, m);
        });
    }

    this.AddNew = function () {
        me.ModelEditorA.StartEdit(new ComplexObject()).done(function (m) {
            me.Records.push(m);
        });
    }
  }

*/

// Example:
// <div title="Update Training Record" style="display: none;" data-bind="jqEditDialog: ModelEditorA">
// where
// ModelEditorA is a ModelEditor object

ko.bindingHandlers.jqEditDialog =
{
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var editor = valueAccessor();

        var defaults = {
            modal: true,
            autoOpen: false,
            height: editor.FormHeight,
            width: editor.FormWidth,
            buttons: {
                Save: function () { editor.CommitEdit(); },
                Cancel: function () { $(this).dialog('close'); }
            },
            close: function () {
                editor.CancelEdit();
            }
        }
        //var options = $.extend(defaults, valueAccessor());
        var options = defaults;

        var setButtons = function (element, enabled) {
            var p = $(element).parent();
            p.find(':button').prop('disabled', !enabled).toggleClass('ui-state-disabled', !enabled);
        }

        $(editor).bind('onBeginEdit', function () {
            $(element).dialog('open');
        });
        $(editor).bind('onEndEdit', function () {
            $(element).dialog('close');
        });
        $(editor).bind('onStartCommitAttempt', function () {
            setButtons(element, false);
        });
        $(editor).bind('onEndCommitAttempt', function () {
            setButtons(element, true);
        });

        $(element).dialog(options);
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {

    }
}

ko.bindingHandlers.jqModalMessage =
{
    init: function (element, valueAccessor, allBindingsAccessor) {
        var options = ko.utils.unwrapObservable(valueAccessor()) || {};
        //do in a setTimeout, so the applyBindings doesn't bind twice from element being copied and moved to bottom
        setTimeout(function () {
            options.close = function () {
                allBindingsAccessor().dialogVisible(false);
            };
            options.modal = true;

            $(element).dialog(options);
        }, 0);

        //handle disposal (not strictly necessary in this scenario)
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            $(element).dialog("destroy");
        });
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        var shouldBeOpen = ko.utils.unwrapObservable(allBindingsAccessor().dialogVisible),
            $el = $(element),
            dialog = $el.data("uiDialog") || $el.data("dialog") || $el.data("ui-dialog");

        //don't call open/close before initilization
        if (dialog) {
            $el.dialog(shouldBeOpen ? "open" : "close");
        }
    }
};


ko.bindingHandlers.jqFileUpload =
{
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var editor = valueAccessor();

        $(element).fileupload({ dataType: 'json' });
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {

    }
}


// factory is a method that returns a new (empty) view model. The <view model>.prototype
// should have a fromJS method that hydrates/clones itself from a plain-javascript object.
var ModelEditor = function (width, height, factory) {
    var me = this;
    var _isEditing = ko.observable(false);
    var _editingObject = null;
    var _deferred;

    this.FormHeight = height;
    this.FormWidth = width;
    this.Model = ko.observable(factory());
    this.IsEditing = ko.computed(function () { return _isEditing(); });

    this.Errors = ko.observableArray();

    this.StartEdit = function (model) {
        if (_isEditing()) return;

        me.Model((model == null) ? factory() : model);

        _deferred = $.Deferred();

        _editingObject = me.Model;

        me.Model(factory().fromJS(ko.mapping.toJS(_editingObject)));
        _isEditing(true);

        $(this).trigger('onBeginEdit');
        return _deferred.promise();
    }

    this.TryCommit = function (model) { var deferred = $.Deferred(); deferred.resolve(model); return deferred.promise(); }

    this.CommitEdit = function () {
        if (!_isEditing()) return;
        
        $(me).trigger('onStartCommitAttempt');
        me.TryCommit(me.Model()).done(function (model) {
            _editingObject = null;
            _isEditing(false);
            $(me).trigger('onEndEdit');
            _deferred.resolve(model);
        }).always(function () { $(me).trigger('onEndCommitAttempt'); });
    }

    this.CancelEdit = function () {
        if (!_isEditing()) return;

        _editingObject = null;
        _isEditing(false);
        $(me).trigger('onEndEdit');
        _deferred.reject();
    }

}

window.locale = {
    "fileupload": {
        "errors": {
            "maxFileSize": "File is too big",
            "minFileSize": "File is too small",
            "acceptFileTypes": "Filetype not allowed",
            "maxNumberOfFiles": "Max number of files exceeded",
            "uploadedBytes": "Uploaded bytes exceed file size",
            "emptyResult": "Empty file upload result"
        },
        "error": "Error",
        "start": "Start",
        "cancel": "Cancel",
        "destroy": "Delete"
    }
}