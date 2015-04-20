/*
 * Copyright 2015 Matthew Cosand
 */
define(['jquery', 'knockout', 'site/utils', 'site/env'], function ($, ko, utils, env) {
  env.registerComponent('contact-addresses', {
    viewModel: function (params) {
      var self = this;
      var linkPrefixes = { phone: 'tel', email: 'mailto' };

      self.addresses = ko.observableArray();
      self.addresses.loading = ko.observable(true);

      self.load = function () {
        self.addresses.loading(true);
        utils.getJSONRetriable('/api/Members/ListAddresses/' + params.memberId).go()
        .done(function (data) {
          self.addresses(data);
        })
        .fail(function (err) { utils.handleServiceError(err, self); })
        .always(function () { self.addresses.loading(false); });
      };

      self.load();
    },
    template: { require: 'text!' + window.appRoot + '/Components/Get/MemberAddresses' }
  });
});