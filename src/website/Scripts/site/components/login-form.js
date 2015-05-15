/*
 * Copyright 2015 Matthew Cosand
 */
define(['jquery', 'knockout', 'site/utils', 'site/env'], function ($, ko, utils, env) {
  env.registerComponent('login-form', {
    viewModel: function (params) {
      var self = this;

      this.onSuccess = params.onSuccess || function () {
        var redirect = utils.qs["returnurl"] || (window.appRoot + '/');
        if (redirect) { env.location.href = redirect; }
        else { env.location.href = env.location.href; }
      };

      this.showLinks = params && params.showLinks;
      this.username = ko.observable();
      this.password = ko.observable();
      this.working = ko.observable(false);

      this._handleResponse = function (r) {
        if (r['errors'] && r['errors'].length > 0) {
          utils.applyErrors(self, r.errors);
          self.working(false);
          return;
        }
        self.onSuccess();
      };

      this.doSubmit = function (formElement) {
        var err = false;
        err = utils.testAndSetError(utils.checkNotNull, self.username, 'Required') || err;
        err = utils.testAndSetError(utils.checkNotNull, self.password, 'Required') || err;
        if (!err) {
          self.working(true);
          utils.postJSON("/api/account/login", ko.toJSON(self))
          .done(self._handleResponse)
          .fail(function (err) {
            utils.handleServiceError(err, self);
            self.working(false);
          })
        }
        return false;
      };

      utils.extendForErrors(this);
    },
    template: { require: 'text!' + window.appRoot + '/Components/Get/LoginForm' }
  });
});