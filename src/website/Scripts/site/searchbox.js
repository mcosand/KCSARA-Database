/*
 * Copyright 2015 Matthew Cosand
 */
ko.components.register('search-box', {
  viewModel: function (params) {
    var self = this;
    this.working = ko.observable(false);
    this.text = ko.observable();
    this.results = ko.observableArray();
    this.ajax = null;
    this.debounce = null;
    this.selected = (params && params.selected) ? params.selected : ko.observable();

    var _searchHandler = function () {
      if (self.debounce) {
        window.clearTimeout(self.debounce);
        self.debounce = window.setTimeout(_processQuery, 300);
      } else _processQuery();
    }

    var _processQuery = function () {
      var query = self.text();
      self.debounce = null;
      self.working(true);
      if (self.ajax) self.ajax.abort();
      self.ajax = App.getJSON('/api/search/find?q=' + encodeURIComponent(query))
        .done(function (data) {
          self.results(data);
        })
        .fail(function (err) { App.handleServiceError(err, self); })
        .always(function () { self.working(false); })
    };

    this.text.subscribe(debounce(_searchHandler, 400, true, self.text));

    this.hasResults = ko.computed(function () { return self.results().length > 0 });

    this.clickItem = function (item) {
      self.results.removeAll();
      self.text(item.text);
      self.selected(item);
    }
    this.doFocus = function () {
      $(window).trigger('resize');
    }
  },
  template: '<form class="form-group has-feedback">' +
             '<label class="control-label sr-only">Search site</label>' +
             '<input type="text" class="form-control" placeholder="Search" data-bind="textInput: text, event: { mouseover: doFocus, focus: doFocus}" />' +
           '<div class="search-box-dropdown glue-previous" data-bind="visible: hasResults, foreach: results">' +
           '<div data-bind="text: text, click: $parent.clickItem" class="search-box-item"></div>' +
           '</div>' +
             '<span class="form-control-feedback fa" data-bind="css: { \'fa-search\': !working(), \'fa-circle-o-notch fa-spin\': working()}"></span>' +
           '</form>'
});