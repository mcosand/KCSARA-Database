define(['knockout', 'moment', 'site/utils', 'signalr-hubs'], function PageModel(ko, moment, utils) {
  return function () {
    var self = this;
    var _currentYear = ko.observable(parseInt($('body').data('year')));
    this.years = ko.observableArray([_currentYear]);
    this.items = ko.observableArray([]);
    this.items.initReset = ko.observable(false).extend({ notify: 'always' });
    this.loadingList = ko.observable(true);
    this.count = ko.observable();
    this.hours = ko.observable();
    this.miles = ko.observable();

    var hub = $.connection.appHub;
    hub.client.eventUpdated = function (theEvent) {
      console.log(theEvent);
      utils.getJSON('/api/' + $('body').data('controller') + '/overview/' + theEvent.id).done(function (data) {
        self.fixupRow(data);
        if (!self.items.replaceById(theEvent.id, data) && (moment(data.start).year() == self.currentYear())) {
          self.items.push(data);
        }
        self.updateTotals();
      });
    };
    $.connection.hub.start({ waitForPageLoad: false });

    this.currentYear = ko.computed({
      read: function () { return _currentYear(); },
      write: function(value) {
        _currentYear(value);
        self.updateMissions();
      }
    });

    this.load = function loadEvents() {
      self.updateMissions();
      utils.getJSON('/api/' + $('body').data('controller') + '/listyears')
        .done(function (data) {
          self.years(data.reverse());
        })
    };

    this.fixupRow = function (row) {
      row.hoursText = row.hours ? row.hours.toFixed(2) : '';
      row.startText = moment(row.start).format('YYYY-MM-DD');
    }
    this.updateTotals = function () {
      var count = 0;
      var hours = 0;
      var miles = 0;
      $.each(self.items(), function (i, el) {
        count++;
        hours += (el.hours || 0.0);
        miles += (el.miles || 0);
      });
      self.count(count);
      self.hours(hours.toFixed(2));
      self.miles(miles);
    }
    this.updateMissions = function() {
      self.loadingList(true);
      utils.getJSON('/api/' + $('body').data('controller') + '/list/' + self.currentYear())
      .done(function (data) {
        $.each(data, function (i, el) {
          self.fixupRow(el);
        });
        self.items.initReset(true);
        self.items(data);
        self.updateTotals();
      })
      .always(function() { self.loadingList(false) });
    }
  };
});
