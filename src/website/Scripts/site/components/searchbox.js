/*
 * Copyright 2015 Matthew Cosand
 */
define(['jquery', 'knockout', 'site/utils', 'site/env'], function($, ko, utils, env) {
  env.registerComponent('search-box', {
    viewModel: function (params) {
      var self = this;
      this.working = ko.observable(false);
      this.text = ko.observable();
      this.results = ko.observableArray();
      this.ajax = null;
      this.debounce = null;
      this.highlighted = ko.observable(-1);
      this.selected = (params && params.selected) ? params.selected : ko.observable();
      this.ctrlId = (params && params.id) ? params.id : undefined;

      var ignoreUpdate = false;
      var _searchHandler = function () {
        if (ignoreUpdate) { return; }
        if (self.debounce) {
          env.clearTimeout(self.debounce);
          self.debounce = env.setTimeout(_processQuery, 300);
        } else _processQuery();
      }

      var _processQuery = function () {
        var query = self.text();
        self.debounce = null;
        self.working(true);
        if (self.ajax) self.ajax.abort();
        self.ajax = utils.getJSONRetriable('/api/search/find?q=' + encodeURIComponent(query)).go()
          .done(function (data) {
            self.results(data);
          })
          .fail(function (err) { utils.handleServiceError(err, self); })
          .always(function () { self.working(false); })
      };

      this.text.subscribe(utils.debounce(_searchHandler, 400, true, self.text));

      this.hasResults = ko.computed(function () {
        return self.results().length > 0
      });

      this.clickItem = function (item) {
        self.results.removeAll();
        ignoreUpdate = true;
        self.text(item.text);
        ignoreUpdate = false;
        self.selected(item);
      }

      this.blur = function (model, evt) {
        env.setTimeout(function () { self.results.removeAll() }, 200);
      }

      this.keyup = function (model, evt) {
        var list = self.results();
        var high = self.highlighted();
        if (evt.keyCode == 40 && high < list.length - 1) {
          self.highlighted(high + 1)
        }
        else if (evt.keyCode == 38 && high > 0) {
          self.highlighted(high - 1)
        }
        else if (evt.keyCode == 13 && high >= 0) {
          self.clickItem(list[high])
        }
      }

      this.doHighlight = function (model, event) {
        var list = self.results();
        for (var i = 0; i < list.length; i++) {
          if (model === list[i]) {
            self.highlighted(i);
            break;
          }
        }
      }

      this.doFocus = function () {
        $(window).trigger('resize');
        var list = self.results();
        if (list.length == 0) { _processQuery(); }
      }
    },
    template: { require: 'text!' + window.appRoot + '/Components/Get/SearchBox' }
  });
});