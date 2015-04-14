/*
 * Copyright 2015 Matthew Cosand
 */
define(['jquery', 'knockout', 'moment', 'site/utils', 'site/env'], function ($, ko, moment, utils, env) {
  env.registerComponent('core-competency-status', {
    viewModel: function (params) {
      var self = this;
    
      $.extend(self, {goodness: ko.observable(true)}, params);

      self.expirations = ko.observable({});
      self.loading = ko.observable(true);
      self.load = function() {
        self.loading(true);
        utils.getJSON(params.api + params.memberId)
        .done(function (data) {
          self.loading(false);
          var e = {};
          for (var id in data.expirations) {
            var row = data.expirations[id];
            e[row.courseName] = row;
          }
          self.expirations(e);
          self.goodness(data.isGood);
        })
        .fail(function (err) { utils.handleServiceError(err, self); })
        .always(function () { self.loading(false); });
      };

      self.getText = function (a, b) {
        var row = self.expirations()[b];
        if (undefined === row) return "Missing";
        if (row.status == "Complete") return row.expires ? new moment(row.expires).format('YYYY-MM-DD') : "Complete";
      };

      self.load();
    },
    template: { require: 'text!' + '' + '/Components/CoreCompetencyStatus' }
  });
});