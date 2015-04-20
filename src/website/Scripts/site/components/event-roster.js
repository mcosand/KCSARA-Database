/*
 * Copyright 2015 Matthew Cosand
 */
define(['jquery', 'knockout', 'moment', 'site/utils', 'site/env'], function ($, ko, moment, utils, env) {
  env.registerComponent('event-roster', {
    viewModel: function (params) {
      var self = this;
      
      $.extend(self, { showBanner: false }, params);

      self.roster = ko.observableArray();
      self.roster.loading = ko.observable(true);
      self.totals = ko.observable({ count: 0, hours: 0.00, miles: 0, hoursText: 0.00 });

      self.load = function () {
        self.roster.loading(true);
        utils.getJSONRetriable(params.api + params.eventId).go()
        .done(function (data) {
          for (var i = 0; i < data.length; i++) {
            var d = data[i];
            var m = moment(d.start);
            d.hoursText = d.hours ? d.hours.toFixed(2) : '';
            d.startText = m.format('YYYY-MM-DD');
          }
          self.roster(data);
        })
        .fail(function (err) { utils.handleServiceError(err, self); })
        .always(function () { self.roster.loading(false); });
      };
      self.load();
    },
    template: { require: 'text!' + window.appRoot + '/Components/Get/EventRoster' }
  });
});