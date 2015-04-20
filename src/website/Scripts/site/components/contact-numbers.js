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
        utils.getJSONRetriable('/api/Members/ListContacts/' + params.memberId).go()
        .done(function (data) {
          for (var i = 0; i < data.length; i++) { var d = data[i]; var p = linkPrefixes[d.type]; d.link = p ? p + ':' + d.value : null };
          self.contacts(data);
        })
        .fail(function (err) { utils.handleServiceError(err, self); })
        .always(function () { self.contacts.loading(false); });
      };

      self.load();
    },
    template: { require: 'text!' + window.appRoot + '/Components/Get/MemberContacts' }
  });
});