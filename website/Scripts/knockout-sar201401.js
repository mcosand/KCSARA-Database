/*
 * Copyright 2014 Matthew Cosand
 */
function mapMoment(item, fields) {
  var items = $.makeArray(item);
  if (fields == undefined) {
    fields = ['T'];
  }
  for (var j in items) {
    for (var i in fields) {
      if (items[j][fields[i]] != undefined) {
        tmp = new moment(items[j][fields[i]]);
        items[j][fields[i]] = tmp;
      }
    }
  }
  return item;
}
function nullableInvoke(method, a, b, c) {
  var tmp = ko.unwrap(method);
  if (tmp != null) tmp(a, b, c);
}
function createMomentObservable(required) {
  var obs = ko.computed({
    read: function () {
      return obs.moment().isValid() ? obs.moment().format() : null;
    },
    owner: this,
    deferEvaluation: true
  });
  obs.moment = ko.observable(moment(new Date()));
  obs.format = ko.observable('YYYY-MM-DD HH:mm');
  obs.errors = ko.observableArray([]);
  obs.invalidInput = ko.observable(null);
  obs.invalid = ko.computed(function () {
    return this.errors().length > 0
  }, obs);
  obs.formatted = ko.computed({
    read: function () {
      var a = obs.moment();
      var b = obs.format();
      return a.isValid() ? a.format(b) : obs.invalidInput();
    },
    write: function (value) {
      var tmp = moment(value);
      obs.invalidInput((tmp.isValid() || value == '') ? null : value);
      obs.moment(moment(value));
    },
    owner: this
  });
  obs.validate = function () {
    obs.errors([]);
    if (obs.invalidInput() != null) obs.errors.push('Invalid date');
    else if (obs() == null && required) obs.errors.push('Required');
  }
  return obs;
}

function validatingObservable(validation) {
  obs = ko.observable();
  obs.hasFocus = ko.observable(false);
  obs.errors = ko.observableArray([]);
  obs.invalid = ko.computed(function () { return obs.errors().length > 0; });
  obs.validate = function () {
    obs.errors([]);
    validation(obs);
  }
  return obs;
}
requiredObservable = function () {
  return validatingObservable(function (obs) { if (obs() == '' || obs() == null) obs.errors.push('Required'); });
}

setOptionDisable = function (option, item) {
  if (item.Id == "disable") $(option).attr('disabled', 'disabled');
}
