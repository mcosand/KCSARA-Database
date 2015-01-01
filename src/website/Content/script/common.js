
if (ko && ko.bindingHandlers) {
    ko.bindingHandlers.jqButtonEnable = {
        'update': function (element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            var $element = $(element);
            $element.prop("disabled", !value);

            if ($element.hasClass("ui-button")) {
                $element.button("option", "disabled", !value);
            }
        }
    };

    ko.bindingHandlers.watermark = {
        init: function (element, valueAccessor, allBindingsAccessor) {
            var value = valueAccessor(), allBindings = allBindingsAccessor();
            var defaultWatermark = ko.utils.unwrapObservable(value);
            var $element = $(element);

            setTimeout(function () {
                if ($element.val() === '') {
                    $element.val(defaultWatermark).addClass('watermark');
                }
            }, 0);

            $element.focus(
                function () {
                    if ($element.val() === defaultWatermark) {
                        $element.val("").removeClass('watermark');
                    }
                }).blur(function () {
                    if ($element.val() === '') {
                        $element.val(defaultWatermark).addClass('watermark');
                    }
                });
        }
    };
}

function formatFileSize(bytes) {
    if (typeof bytes !== 'number') {
        return '';
    }
    if (bytes >= 1000000000) {
        return (bytes / 1000000000).toFixed(2) + ' GB';
    }
    if (bytes >= 1000000) {
        return (bytes / 1000000).toFixed(2) + ' MB';
    }
    return (bytes / 1000).toFixed(2) + ' KB';
}

function formatDateTime(format, date, settings)
{
    if (!date) { return "" }
    // var dayNamesShort = (settings ? settings.dayNamesShort : null) || this._defaults.dayNamesShort;
    // var dayNames = (settings ? settings.dayNames : null) || this._defaults.dayNames;
    // var monthNamesShort = (settings ? settings.monthNamesShort : null) || this._defaults.monthNamesShort;
    // var monthNames = (settings ? settings.monthNames : null) || this._defaults.monthNames;
    var lookAhead = function(match) {
        var matches = (iFormat + 1 < format.length && format.charAt(iFormat + 1) == match);
        if (matches) { iFormat++ } return matches
    };

    var formatNumber = function(match, value, len) {
        var num = "" + value;
        if (lookAhead(match)) {
            while (num.length < len) {
                num = "0" + num
            }
        }
        return num
    };
    var formatName = function(match, value, shortNames, longNames) {
        return (lookAhead(match) ? longNames[value] : shortNames[value])
    };
    var output = "";
    var literal = false;
    if (date) {
        for (var iFormat = 0; iFormat < format.length; iFormat++) {
            if (literal) {
                if (format.charAt(iFormat) == "'" && !lookAhead("'")) {
                    literal = false
                } else {
                    output += format.charAt(iFormat)
                }
            } else {
                switch (format.charAt(iFormat)) {
                    case "d": output += formatNumber("d", date.getDate(), 2); break;
                    case "D": output += formatName("D", date.getDay(), ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"], ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]); break;
                        //    case "o": var doy = date.getDate(); for (var m = date.getMonth() - 1; m >= 0; m--) { doy += this._getDaysInMonth(date.getFullYear(), m) } output += formatNumber("o", doy, 3); break;
                    case "m": output += formatNumber("m", date.getMonth() + 1, 2); break;
                        //    case "M": output += formatName("M", date.getMonth(), monthNamesShort, monthNames); break;
                    case "y": output += (lookAhead("y") ? date.getFullYear() : (date.getYear() % 100 < 10 ? "0" : "") + date.getYear() % 100); break;
                    case "@": output += date.getTime(); break;
                    case "'": if (lookAhead("'")) { output += "'" } else { literal = true } break;
                    case 'H': output += formatNumber("H", date.getHours(), 2); break;
                    case 'i': output += formatNumber('i', date.getMinutes(), 2); break;
                    default: output += format.charAt(iFormat)
                }
            }
        }
    }
    return output
};


Date.prototype.toJSON = function (key) {
    return isFinite(this.valueOf()) ? '\/Date(' + this.getTime() + this.getUTCOffset() + ')\/' : null;
};
// IE7 .getUTCOffset returns 'undefined0700'
Date.prototype.getUTCOffset = function () {
    var f = this.getTimezoneOffset();
    return (f < 0 ? '+' : '-') + ((f / 60 < 10) ? '0' : '') + f / 60 + ((f % 60 < 10) ? '0' : '') + f % 60;
};

function ifDefined(value) { return (value == undefined) ? "" : value; }
function ifNumeric(value) { return isNaN(value) ? "" : value; }

function fixTime(item, fields) {
    if (fields == undefined) {
        fields = ['T'];
    }
    for (var i in fields) {
        if (item[fields[i]] != undefined) {
            var tmp = /\/Date\((\d+).*/.exec(item[fields[i]]);
            item[fields[i]] = (tmp == null) ? Date.parse('1800-01-01') : new Date(parseInt(tmp[1]));
        }
    }
    return item;
}
function makeDates(items, fields) {
    for (var i = 0; i < items.length; i++) {
        for (var f in fields) {
            if (items[i][fields[f]] != undefined && items[i][fields[f]] != null && typeof items[i][fields[f]] == "string")
                items[i][fields[f]] = Date.parse(items[i][fields[f]].replace(/\.\d+$/,''));
        }
    }
    return items;
}

function parseCoordinate(coord) {
    if (coord == undefined || coord == "") return null;

    var m = /^\-?(\d{2,3}(\.\d+)?)$/.exec(coord);
    if (m != null) { return m[1]; }

    m = /^\-?(\d{2,3}) (\d{1,2}(\.\d+)?)$/.exec(coord);
    if (m != null) { return Math.floor(m[1]) + m[2] / 60.0; }

    m = /\-?(\d{2,3}) (\d{1,2}) (\d{1,2}(\.\d+)?)$/.exec(coord);
    if (m != null) { return Math.floor(m[1]) + (Math.floor(m[2]) + m[3] / 60.0) / 60.0; }

    return undefined;
}
function isDefined(variable) {
    return (typeof (window[variable]) == "undefined") ? false : true;
}

function isPropertyDefined(variable, property) {
    return (variable == undefined) ? false : (variable[property] != undefined);
}

var ModelTables = [];

function ModelTable(tableId, formid) {

  this.tableId = tableId;
  this.formId = formid;
  this.canEdit = false;
  this.height = 400;
  this.width = 450;
  this.defaultSort = [];
  this.itemKey = 'Id';
  this.objectType = 'unknown';
  this.unpacker = function (data) { return data; };
  this.dateTimeFields = ['T'];
  this.onSubmitSuccess = function () { return true; };
  this.id = ModelTables.length;
  this.usesAPI = false;
  ModelTables[this.id] = this;
}

ModelTable.prototype.Initialize = function()
{
  this.WireForm();
  $(this.tableId).tablesorter({ widgets: ['zebra'] });
  this.ReloadData();
};

ModelTable.prototype.EditPrompt = function (itemId) {
    this.current = this.store[this.storeIndexOf(this.store, itemId)];
    this._showDialog();
};

ModelTable.prototype.CreatePrompt = function (newItem) {
    this.current = newItem;
    this._showDialog();
};

ModelTable.prototype._showDialog = function () {
    if (!this.fillForm(this.current)) {
        return;
    }
    var form = $(this.formId);
    form.dialog('open');
    form.dialog('option', 'position', 'center');
};

ModelTable.prototype.DeletePrompt = function(itemId, row)
{
  var del = $('#formsDelete');
  del[0].table = this;
  del[0].kcsaraItemId = itemId;

  $('#ui-dialog-title-formsDelete').html("Delete " + this.objectType);
  $('#formsDeleteTitle').html(this.objectType);
  del.dialog('open');
  del.dialog('option', 'position', 'center');

};

ModelTable.prototype.ReloadData = function () {
    var me = this;
    $.ajax({ type: 'POST', url: this.getUrl, data: null, dataType: 'json',
        success: function (data) {
            var items = me.unpacker(data);
            me.store = items;
            $(me.tableId + ' tbody tr').remove();
            for (i in items) {
                var item = items[i];
                fixTime(item, me.dateTimeFields);
                me.AppendRow(items[i], false);
            }
            me.UpdateTable();
            if (me.onUpdated) me.onUpdated(data);
        },
        error: handleDataActionError
    });
};

ModelTable.prototype.RenderRow = function(item, row)
{
  this.renderer(item, row);
  if (this.canEdit)
  {
     row.append(
     '<td><a href="#" onclick="ModelTables['+this.id+'].EditPrompt(\'' + item[this.itemKey] + '\'); return false;">Edit</a> ' +
    '<a href="#" onclick="ModelTables['+this.id+'].DeletePrompt(\'' + item[this.itemKey] + '\',$(this).parent().parent()); return false;">Delete</a>'+
    '</td>');
  }
}

ModelTable.prototype.AppendRow = function(item, refreshSort)
{
  row = $('<tr id="i'+item[this.itemKey]+'"></tr>');
  this.RenderRow(item, row);

  $(this.tableId + ' > tbody:last').append(row);
  
  if (refreshSort)
  {
      this.UpdateTable();
  }
};

ModelTable.prototype.UpdateTable = function()
{
  if ($(this.tableId + " tbody")[0].children.length == 0)
  {
    return;
  }

  var table = $(this.tableId);
  var list = table[0].config.sortList;
  if (list.length == 0)
  {
    list = this.defaultSort;
  }

  table.trigger("update");
  table.trigger("sorton", [list]);
  table.trigger("applyWidgets");
};

ModelTable.prototype.storeIndexOf = function(store, key) {
    for (var i in store) {
        if (store[i][this.itemKey] == key) return i;
    }
}

ModelTable.prototype.WireForm = function () {
    var me = this;

    $(this.formId).dialog({
        autoOpen: false,
        height: me.height,
        width: me.width,
        modal: true,
        buttons: {
            'Save': function () {

                $('.ui-dialog-buttonpane button').attr('disabled', 'true').addClass('ui-state-disabled');

                $(me.formId + ' input,select').removeClass('ui-state-error');
                $(me.formId + ' .validateTips').removeClass('ui-state-error');
                $(me.formId + ' .validateTips').html('');

                var d = $(this);
                var result = null;

                var toSubmit = me.formParser(me.current);

                $.ajax({ type: 'POST', url: me.postUrl, data: JSON.stringify(toSubmit), dataType: 'json', contentType: 'application/json; charset=utf-8',
                    success: function (data) {
                        if (me.usesAPI || data.Errors.length == 0) {
                            result = me.usesAPI ? data : data.Result;

                            var doClose = true;
                            if (result != null) {
                                fixTime(result, me.dateTimeFields);
                                if (result[me.itemKey] == me.current[me.itemKey]) {
                                    me.store[me.storeIndexOf(me.store, me.current[me.itemKey])] = result;
                                    me.RenderRow(result, $('#i' + result[me.itemKey]));
                                }
                                else {
                                    me.store.push(result);
                                    me.AppendRow(result, true);
                                }
                                doClose = me.onSubmitSuccess(result);
                            }

                            if (doClose) d.dialog('close');
                        }
                        else if (data.Errors.length > 0) {
                            var tips = $(me.formId + ' .validateTips');
                            for (var i in data.Errors) {
                                var error = data.Errors[i];
                                var ctrlId = null;

                                tips.append('<span style="color:red; margin-left:1em;">' + error.Property + ': ' + error.Error + '</span>');
                                tips.addClass('ui-state-error');
                            }
                        }
                    },
                    error: handleDataActionError,
                    complete: function (request, status) {
                        $('.ui-dialog-buttonpane button').removeAttr('disabled').removeClass('ui-state-disabled');
                    }
                });
            },
            Cancel: function () {
                $(this).dialog('close');
            }
        },
        close: function () {
            $(this.formId + ' input,select').removeClass('ui-state-error');
            $('label > span').remove();
        }
    });
};

var formsSetup = false;
function formsInit()
{
    if (formsSetup) return;
    
    formsSetup = true;
    $("#formsDelete").dialog({
      autoOpen: false,
      height: 180,
      width: 450,
      modal: true,
      buttons: {
        'Delete': function () {
          $('.ui-dialog-buttonpane button').attr('disabled', 'true').addClass('ui-state-disabled');
          var d = $(this);
          
          var mt = $("#formsDelete")[0].table;
          var itemId = $("#formsDelete")[0].kcsaraItemId;

          $.ajax({ type: 'POST', url: mt.deleteUrl, data: { id: itemId }, dataType: 'json',
            success: function (data) {
              if (data.Errors.length == 0) {
                d.dialog('close');
                if (mt.onDelete == undefined)
                {
                  $('#i' + itemId).remove();
                  mt.UpdateTable();
                }
                else
                {
                  mt.onDelete(itemId);
                }
              }
            },
            error: handleDataActionError,
            complete: function (request, status) {
              $('.ui-dialog-buttonpane button').removeAttr('disabled').removeClass('ui-state-disabled');
            }
          });
        },
        Cancel: function () {
          $(this).dialog('close');
        }
      }
    });
    
    $("#formsLogin").dialog({
    autoOpen: false,
    height: 280,
    width: 450,
    modal: true,
    buttons: {
      'Login': function () {
        $('.ui-dialog-buttonpane button').attr('disabled', 'true').addClass('ui-state-disabled');
        var d = $(this);
        $.ajax({ type: 'POST', url: kcsarBaseUrl + '/account/servicelogin', data: { username: $('#formsUsername').val(), password: $('#formsPassword').val() }, dataType: 'json',
          success: function (data) {
            if (data.Errors.length == 0) {
              d.dialog('close');
            }
          },
          error: handleDataActionError,
          complete: function (request, status) {
            $('.ui-dialog-buttonpane button').removeAttr('disabled').removeClass('ui-state-disabled');
          }
        });
      },
      Cancel: function () {
        $(this).dialog('close');
      }
    }
  });
}

function handleDataActionError(request, status, error)
{
      if (request.status == 403 && request.responseText == 'login')
      {
        $('#formsLogin').dialog('open');
        $('#formsUsername').focus();
        $('#formsLogin').dialog('option', 'position', 'center');
      }
      else if (request.status != 0)
      {
        alert('Error submitting request:\n  ' + request + '::' + status + '::' + error);
      }
}

var dateTimePickers = new Array();
function applyDTP(id, st) {
    $('#' + id).datepicker({ dateFormat: 'yy-mm-dd', changeMonth: true, changeYear: true, showOn: 'button', buttonImage: kcsarBaseUrl+'Content/images/calendar.gif', buttonImageOnly: true, constrainInput: false, showTime: st, time24h: true, duration: '' });
}

var kcsarBaseUrl = "/";

function kcsarInit(baseUrl) {
    kcsarBaseUrl = baseUrl;
    $(".datepicker").each(function (index, element) {
        applyDTP(element.id, false);
    });
    $(".datetimepicker").each(function (index, element) {
        applyDTP(element.id, true);
    });
    formsInit();
    $(".zebra-sorted").tablesorter({ widgets: ['zebra'] });
}

function getApiFailureHandler(loadingObservable)
{
    var obsv = loadingObservable;
    return function (err) {
        var msg = (err.responseJSON) ? err.responseJSON.Message : err.responseText;
        alert(msg);
        if (obsv) obsv(false);
    }
}