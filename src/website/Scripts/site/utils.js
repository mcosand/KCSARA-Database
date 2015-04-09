define(['jquery', 'knockout', 'site/env'], function ($, ko, env) {
  var utils = {};
  utils.getCookie = function getCookie(name, source) {
    var src = source || env.cookie;
    if (!src) return null;
    var searched = src.split(';').map(function (e) { return e.trim() }).filter(function (s) {
      return s.search(name + '=') == 0
    })
    if (searched.length == 0) return undefined;
    return searched[0].split(/(=)/).slice(2).join('')
  };

  utils.addKOLoader = function addKOLoader(obs) {
    obs.load = function loader(data) {
      if (data == null) return;
      for (var p in obs) {
        if (ko.isObservable(obs[p]) && data[p] !== undefined) {
          if (/*object is simple*/true) { obs[p](data[p]); }
        }
      }
      return obs;
    }
  };

  utils.debounce = function debounce(func, wait, immediate, stateFunc) {
    var timeout;
    var previousState = null;
    return function () {
      var context = this, args = arguments;
      var later = function () {
        timeout = null;
        if (stateFunc) {
          var now = stateFunc();
          if (previousState != now) { previousState = now; func.apply(context, args); }
        } else if (!immediate) {
          func.apply(context, args);
        }
      };
      var callNow = immediate && !timeout;
      env.clearTimeout(timeout);
      timeout = env.setTimeout(later, wait);
      if (callNow) {
        if (stateFunc) previousState = stateFunc();
        func.apply(context, args);
      }
    };
  };

  utils.postJSON = function postJSON(relativeUrl, data) {
    return $.ajax('' + relativeUrl, { type: 'POST', contentType: 'application/json', dataType: 'json', data: data });
  };
  utils.getJSON = function getJSON(relativeUrl, data) {
    return $.ajax('' + relativeUrl, { type: 'GET', contentType: 'application/json', dataType: 'json', data: data });
  };
  utils.log = function (message) {
    env.log(message);
  };

  utils.applyErrors = function applySubmitErrors(errs) {
    for (var i = 0; i < errs.length; i++) {

      if (errs[i].property === undefined || errs[i].property === null || errs[i].property === '') {
        self.error(errs[i].text);
      }
      else {
        self[errs[i]].error(errs[i].text);
      }
    }
  };

  utils.handleServiceError = function handleServiceError(err, model) {
    if (err.status == 0) return;
    if (err.status == 401) {

    } else {
      utils.log(err);
      if (err.responseJSON) {
        if (model && ko.isObservable(model.error)) {
          var val = "";
          for (var p in err.responseJSON) {
            if (model[p] && ko.isObservable(model[p]['error'])) {
              model[p].error(err.responseJSON[p]);
            } else {
              val += err.responseJSON[p] + "\n";
            }
          }
          model.error(val);
          return;
        }
      }
      $.toaster({ title: 'Error', priority: 'danger', message: 'An error occured talking to server' })
    }
  };

  utils.testAndSetError = function testAndSetError(checker, observable, message) {
    observable.error(null);
    if (checker(observable())) {
      observable.error(message);
      return true;
    }
    return false;
  };

  utils.checkNotNull = function checkNotNull(value) { return value == null; };

  utils.extendForErrors = function extendForErrors(model) {
    if (model == null) return;
    for (var p in model) {
      if (ko.isObservable(model[p]) && model[p]['error'] === undefined) { model[p].error = ko.observable(); }
    }
    if (model.error === undefined) model.error = ko.observable();
  };

  var searchParts = env.location.search.substr(1).split('&');
  utils.qs = (function (a) {
    if (a == "") return {};
    var b = {};
    for (var i = 0; i < a.length; ++i) {
      var p = a[i].split('=', 2);
      if (p.length == 1)
        b[p[0].toLowerCase()] = "";
      else
        b[p[0].toLowerCase()] = decodeURIComponent(p[1].replace(/\+/g, " "));
    }
    return b;
  })(searchParts);

  return utils;
});