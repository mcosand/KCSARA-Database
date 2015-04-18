/*
 * Copyright 2015 Matthew Cosand
 */
define(['jquery', 'knockout', 'moment', 'site/utils', 'site/env'], function ($, ko, moment, utils, env) {
  env.registerComponent('member-events', {
    viewModel: function (params) {
      var self = this;
      
      $.extend(self, { showBanner: false }, params);

      self.events = ko.observableArray();
      self.events.loading = ko.observable(true);
      self.totals = ko.observable({ count: 0, hours: 0.00, miles: 0, firstYear: new Date().getYear(), hoursText: 0.00 });
      self.load = function () {
        self.events.loading(true);
        utils.getJSON(params.api + params.memberId)
        .done(function (data) {
          var firstYear = new Date().getYear() + 1900;
          for (var i = 0; i < data.rows.length; i++) {
            var d = data.rows[i];
            var m = moment(d.start);
            if (m.year() < firstYear) firstYear = m.year();
            d.hoursText = d.hours ? d.hours.toFixed(2) : '';
            d.startText = m.format('YYYY-MM-DD');
            d.title = d.title.replace("-", "&#8209;");
          }
          data.totals.firstYear = firstYear;
          if (data.totals.hours) data.totals.hoursText = data.totals.hours.toFixed(2);
          self.totals(data.totals);
          self.events(data.rows);
        })
        .fail(function (err) { utils.handleServiceError(err, self); })
        .always(function () { self.events.loading(false); });
      };
      self.load();
    },
    template: { require: 'text!' + window.appRoot + '/Components/Get/MemberEvents' }
  });
});