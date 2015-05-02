/*
 * Copyright 2015 Matthew Cosand
 */
define(['jquery', 'knockout', 'moment', 'pagedown-convert', 'site/utils', 'site/env'], function ($, ko, moment, Markdown, utils, env) {
  env.registerComponent('event-timeline', {
    viewModel: function (params) {
      var self = this;
      
      $.extend(self, params);

      self.timeline = ko.observableArray();
      self.timeline.loading = ko.observable(true);

      self.filteredEvents = ko.observableArray(['EventLog']);

      self.filteredTimeline = ko.computed(function () {
        if (!self.filteredEvents()) {
          return self.timeline();
        } else {
          var filtered = self.filteredEvents();
          return ko.utils.arrayFilter(self.timeline(), function (row) {
            console.log({ test: filtered.indexOf(row), row: row.type });
            return filtered.indexOf(row.type) >= 0;
          });
        }
      });

      self.formatTime = function formatTime(time) {
        var days = time.diff(self.topInfo().start, 'days');
        var retVal = time.format("HHmm");
        if (days < 0) { retVal =  (-days) + '-' + revVal }
        else if (days > 0) { retVal = days + '+' + retVal }
        return retVal;
      };

      self._converter = new Markdown.Converter();
      self.formatMarkdown = function formatMarkdown(markdown) {
        return self._converter.makeHtml(markdown);
      };

      self.load = function () {
        self.timeline.loading(true);
        utils.getJSONRetriable(params.api + params.eventId).go()
        .done(function (data) {
          for (var i = 0; i < data.length; i++) {
            var d = data[i];
            d.time = new moment(d.time);
          }
          self.timeline(data);
        })
        .fail(function (err) { utils.handleServiceError(err, self); })
        .always(function () { self.timeline.loading(false); });
      };
      self.load();
    },
    template: { require: 'text!' + window.appRoot + '/Components/Get/EventTimeline' }
  });
});