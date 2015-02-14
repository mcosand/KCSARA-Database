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
    this.highlighted = ko.observable(-1);
    this.selected = (params && params.selected) ? params.selected : ko.observable();

    var ignoreUpdate = false;
    var _searchHandler = function () {
      if (ignoreUpdate) { return; }
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
      ignoreUpdate = true;
      self.text(item.text);
      ignoreUpdate = false;
      self.selected(item);
    }

    this.blur = function () {
      window.setTimeout(self.results.removeAll, 5);
    }

    this.keyup = function (model, evt) {
      var val = self.text();
      var list = self.results();
      var high = self.highlighted();
      if (evt.keyCode == 40 && high < list.length - 1) { self.highlighted(high + 1) }
      else if (evt.keyCode == 38 && high > 0) { self.highlighted(high - 1) }
      else if (evt.keyCode == 13 && high >= 0) { self.clickItem(list[high]) }
    }

    this.doHighlight = function(model, event)
    {
      console.log('doing highlight');
      var list = self.results();
      console.log(list);
      console.log(model);
      for (var i = 0; i < list.length; i++)
      {
        if (model === list[i]) { self.highlighted(i); }
      }
    }

    this.doFocus = function () {
      $(window).trigger('resize');
      var list = self.results();
      if (list.length == 0) { _processQuery(); }
    }
  },
  template: '<form class="form-group has-feedback">' +
              '<label class="control-label sr-only">Search site</label>' +
              '<input type="text" class="form-control" placeholder="Search" data-bind="textInput: text, event: { keyup: keyup, focus: doFocus, blur: blur}" />' +
              '<div class="search-box-dropdown glue-previous" data-bind="visible: hasResults, foreach: results">' +
                '<div data-bind="click: $parent.clickItem, event: {mouseover: $parent.doHighlight}, css: { \'search-box-highlight\': $parent.highlighted() == $index() }" class="search-box-item">' +
                    '<img data-bind="attr: { src: appRoot + \'/api/members/getthumbnail/\' + more.id + \'?width=25\' }" />' +
                    '<div><strong data-bind="text: text"></strong><span data-bind="visible: more.dem, text: \' -  \' + more.dem"></span></div>' +
                '</div>' +
              '</div>' +
              '<span class="form-control-feedback fa" data-bind="css: { \'fa-search\': !working(), \'fa-circle-o-notch fa-spin\': working()}"></span>' +
            '</form>'
});