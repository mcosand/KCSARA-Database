/*
 * Copyright 2015 Matthew Cosand
 */
(function buildApp() {
  var addLoader = function addLoader (obs) {
    obs.load = function loader(data) {
      if (data == null) return;
      for (p in obs) {
        if (ko.isObservable(obs[p]) && data[p] !== undefined) {
          if (/*object is simple*/true) { obs[p](data[p]); }
        }
      }
      return obs;
    }
  }

  UserModel = function () {
    var self = this;
    this.username = ko.observable();
    this.firstname = ko.observable();
    this.isAuthenticated = ko.observable();
    addLoader(this);
  }

  App = function () {
    var self = this;
    this.page = null;
    this.searchResult = ko.observable();
    this.searchResult.subscribe(function (newValue) {
      window.location.href = newValue.url;
    })
    this.user = ko.observable(new UserModel());
  }

  App.log = function (msg) { console.log(msg); }

  App.bootstrap = function (pageModel) {
    var authInfo = JSON.parse(App.getCookie("authInfo") || "{}");
    if (authInfo) {
      document.cookie = 'authInfo=;expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    }

    var _resizeGlued = function()
    {
      var el = $(this);
      var anchor = el.data('anchorNode');
      var offset = anchor.offset();
      el.css({ 'position': 'absolute', 'left': offset.left + 'px', 'top': (offset.top + anchor.outerHeight()) + 'px', 'width': Math.max(anchor.outerWidth(), 200) + "px", 'z-index': 1040 });
    }
    var _setupGlued = function()
    {
      var el = $(this);
      el.data('anchorNode', el.prev());
      $(document.body).append(el.detach());
    }

    return function () {
      var model = new App();
      model.user().load(authInfo);
      model.page = pageModel;
      ko.applyBindings(model);
      $(window).on('resize', function () { $('.glue-previous').each(_resizeGlued) });
      window.setTimeout(function () { $('.glue-previous').each(_setupGlued); $(window).trigger('resize') }, 5);
    };
  }

  App.postJSON = function postJSON(relativeUrl, data) {
    return $.ajax(window.appRoot + relativeUrl, { type: 'POST', contentType: 'application/json', dataType: 'json', data: data });
  }
  App.getJSON = function postJSON(relativeUrl, data) {
    return $.ajax(window.appRoot + relativeUrl, { type: 'GET', contentType: 'application/json', dataType: 'json', data: data });
  }

  App.testAndSetError = function testAndSetError(checker, observable, message) {
    observable.error(null);
    if (checker(observable())) {
      observable.error(message);
      return true;
    }
    return false;
  }
  App.checkNotNull = function checkNotNull(value) { return value == null; }

  App.extendForErrors = function extendForErrors(model) {
    if (model == null) return;
    for (p in model) {
      if (ko.isObservable(model[p]) && model[p]['error'] === undefined) { model[p].error = ko.observable(); }
    }
    if (model.error === undefined) model.error = ko.observable();
  }

  App.handleServiceError = function handleServiceFailure(err, model)
  {
    if (err.status == 0) return;
    App.log(err);
    if (err.responseJSON) {
      if (model && ko.isObservable(model.error)) {
        val = "";
        for (var p in err.responseJSON) {
          val += err.responseJSON[p] + "\n";
        }
        model.error(val);
        return;
      }
    }
    $.toaster({ title: 'Error', priority: 'danger', message: 'An error occured talking to server' })
  }

  App.getCookie = function getCookie(name, source) {
    var cookies = (source || document.cookie).split(';');
    for (var i = 0; i < cookies.length; i++) {
      var arr = cookies[i].split('=');
      if (arr[0] == name) return arr[1];
    }
  }

  App.qs = (function (a) {
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
  })(window.location.search.substr(1).split('&'));

  App.addLoader = addLoader;
  App._rebuildApp = buildApp;
})();

$.toaster({ settings: { timeout: 3000, donotdismiss: ['danger'] } });
window.onerror = function (err, msg, loc) {
  var packet = { Error: err, Message: msg, Location: loc };
  App.log(packet);
  App.postJSON("/api/telemetry/error", JSON.stringify(packet));
};