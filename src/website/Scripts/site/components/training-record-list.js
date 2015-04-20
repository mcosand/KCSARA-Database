/*
 * Copyright 2015 Matthew Cosand
 */
define(['jquery', 'knockout', 'moment', 'site/utils', 'site/env'], function ($, ko, moment, utils, env) {
  env.registerComponent('training-record-list', {
    viewModel: function (params) {
      var self = this;
      
      $.extend(self, { showBanner: false }, params);

      self.records = ko.observableArray();
      self.records.loading = ko.observable(true);
      self.load = function () {
        self.records.loading(true);
        utils.getJSONRetriable(params.api + params.memberId).go()
        .done(function (data) {
          for (var i = 0; i < data.length; i++) {
            var d = data[i];
            d.completedText = new moment(d.completed).format("YYYY-MM-DD");
            d.expiryText = d.expiry ? new moment(d.expiry).format("YYYY-MM-DD") : "Complete";
          }
          self.records(data);
        })
        .fail(function (err) { utils.handleServiceError(err, self); })
        .always(function () { self.records.loading(false); });
      };
      self.load();
    },
    template: { require: 'text!' + window.appRoot + '/Components/Get/TrainingRecordList' }
  });
});