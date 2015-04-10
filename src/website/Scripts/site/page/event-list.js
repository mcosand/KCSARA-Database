define(['knockout', 'moment', 'site/utils'], function PageModel(ko, moment, utils) {
  return function () {
    var self = this;
    var _currentYear = parseInt($('body').data('year'));
    this.years = ko.observableArray([_currentYear]);
    this.items = ko.observableArray([]);
    this.loadingList = ko.observable(true);
    this.count = ko.observable();
    this.hours = ko.observable();
    this.miles = ko.observable();

    this.currentYear = ko.computed({
      read: function() { return _currentYear; },
      write: function(value) {
        _currentYear = value;
        self.updateMissions();
      }
    });

    this.load = function loadEvents() {
      self.updateMissions();
      utils.getJSON('/api/' + $('body').data('eventType') + '/listyears')
        .done(function (data) {
          self.years(data.reverse());
        })
    };
    this.updateMissions = function() {
      self.loadingList(true);
      utils.getJSON('/api/' + $('body').data('eventType') + '/list/' + _currentYear)
      .done(function (data) {
        var count = 0;
        var hours = 0;
        var miles = 0;
        $.each(data, function (i, el) {
          el.hoursText = el.hours ? el.hours.toFixed(2) : '';
          el.startText = moment(el.start).format('YYYY-MM-DD');
          count++;
          hours += (el.hours || 0.0);
          miles += (el.miles || 0);
        });
        self.count(count);
        self.hours(hours.toFixed(2));
        self.miles(miles);
        self.items(data);
      })
      .always(function() { self.loadingList(false) });
    }
  };
});
