/*
 * Copyright 2015 Matthew Cosand
 */
LoginModel = function () {
  var self = this;
  this.username = ko.observable();
  this.password = ko.observable();
  this.working = ko.observable(false);

  function applyErrors(errs) {
    for (var i=0; i<errs.length; i++)
    {
      
      if (errs[i].property === undefined || errs[i].property === null || errs[i].property === '') {
        self.error(errs[i].text);
      }
      else {
        self[errs[i]].error(errs[i].text);
      }
    }
  }

  this._handleResponse = function (r) {
    if (r.errors && r.errors.length > 0) {
      applyErrors(r.errors);
      return;
    }
    var redirect = App.qs["returnurl"];
    if (redirect) { window.location.href = redirect; }
    else { window.location.href = window.location.href; }
  };

  this.doSubmit = function (formElement) {
    var err = false;
    err = App.testAndSetError(App.checkNotNull, self.username, 'Required') || err;
    err = App.testAndSetError(App.checkNotNull, self.password, 'Required') || err;
    if (!err) {
      self.working(true);
      App.postJSON("/api/account/login", ko.toJSON(self))
      .done(self._handleResponse)
      .fail(function(err) { App.handleServiceError(err, self) })
      .always(function () { self.working(false); })
    }
    return false;
  };

  App.extendForErrors(this);
}