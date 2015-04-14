/*
 * Copyright 2015 Matthew Cosand
 */
define(['jquery', 'knockout', 'site/utils', 'site/env'], function ($, ko, utils, env) {
  env.registerComponent('contact-numbers', {
    viewModel: function (params) {
      var self = this;
      var linkPrefixes = { phone: 'tel', email: 'mailto' };

      self.contacts = ko.observableArray();
      self.contacts.loading = ko.observable(true);

      self.load = function () {
        self.contacts.loading(true);
        utils.getJSON('/api/Members/ListContacts/' + params.memberId)
        .done(function (data) {
          for (var i = 0; i < data.length; i++) { var d = data[i]; var p = linkPrefixes[d.type]; d.link = p ? p + ':' + d.value : null };
          self.contacts(data);
        })
        .fail(function (err) { utils.handleServiceError(err, self); })
        .always(function () { self.contacts.loading(false); });

        //self.addresses.loading(true);
        //utils.getJSON('/api/MemberAddresses/List/' + params.memberId)
        //.done(function (data) {
        //  self.addresses(data);
        //})
        //.fail(function (err) { utils.handleServiceError(err, self); })
        //.always(function () { self.addresses.loading(false); });
      };

      self.load();
      /*

      this._handleResponse = function (r) {
        if (r['errors'] && r['errors'].length > 0) {
          utils.applyErrors(r.errors);
          return;
        }
        var redirect = utils.qs["returnurl"];
        if (redirect) { env.location.href = redirect; }
        else { env.location.href = env.location.href; }
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
      */
    },
    template: { require: 'text!' + '' + '/Components/MemberContacts' }
  });
});