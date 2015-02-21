define(['testlib/squire', 'jquery', 'knockout', 'site/env'], function (Squire, prodJQuery, ko, prodEnv) {
  window.pushSquire(function (squireDone) {
    var mockQuery = {
      lastCall: null,
      ajaxPromise: null
    };
    mockQuery.ajax = function (url, args) {
      mockQuery.ajaxPromise = prodJQuery.Deferred();
      mockQuery.lastCall = { url: url, args: args };
      return mockQuery.ajaxPromise.promise();
    }

    var mockEnv = prodJQuery.extend({}, prodEnv);
    mockEnv.location = { href: 'foo', search: '' };

    new Squire()
      .mock('jquery', mockQuery)
      .mock('knockout', ko) // use the same knockout for everyone.
      .mock('site/env', mockEnv)
      .require(["site/components/searchbox", 'knockout'], function (searchbox, testKO) {
        module('site/components/searchbox');
        var Searchbox = mockEnv.registeredComponents['search-box'];
        // ------------------------------------------------
        test("is registered", function () {
          notEqual(typeof Searchbox, 'undefined');
        });

        // ------------------------------------------------
        test("view model init - defaults", function () {
          var model = new Searchbox.viewModel();
          ok(testKO.isObservable(model.working), 'working observable');
          equal(model.working(), false, 'working initial state');

          ok(testKO.isObservable(model.text), 'text observable');
          equal(model.text(), undefined, 'text initial state');

          ok(testKO.isObservable(model.results) && 'push' in model.results, 'results array');

          ok(testKO.isObservable(model.highlighted), 'highlighted observable');
          equal(model.highlighted(), -1, 'highlighted initial value');

          ok(testKO.isObservable(model.selected), 'selected observable');
          equal(model.selected(), undefined, 'selected initial value');
        });

        test("blur", function (assert) {
          var done = assert.async();
          var timeout = mockEnv.setTimeout;
          var model = new Searchbox.viewModel();
          model.results.push('thing a');
          model.results.push('thing b');
          mockEnv.setTimeout = function (fn, t) {
            mockEnv.setTimeout = timeout;
            ok(t > 100, 'sufficient timeout');
            fn();
            equal(model.results().length, 0, 'list emptied');
            done();
          }
          model.blur(model, {});
        });

        test("keyup - up - empty list", function () {
          var model = new Searchbox.viewModel();
          equal(model.highlighted(), -1, 'before');
          model.keyup(model, { keyCode: 38 });
          equal(model.highlighted(), -1, 'after');
        });

        test("keyup - move up", function () {
          var model = new Searchbox.viewModel();
          model.results.push('new result');
          model.results.push('other result');
          model.highlighted(1);
          model.keyup(model, { keyCode: 38 });
          equal(model.highlighted(), 0, 'updated');
        });

        test("keyup - move down", function () {
          var model = new Searchbox.viewModel();
          model.results.push('new result');
          model.keyup(model, { keyCode: 40 });
          equal(model.highlighted(), 0, 'updated');
        });

        test('keyup - enter - click item', function () {
          var model = new Searchbox.viewModel();
          var item = { text: 'new result' };
          model.results.push(item);
          model.highlighted(0);
          model.keyup(model, { keyCode: 13 });
          equal(model.results().length, 0, 'final results length');
          equal(model.text(), 'new result', 'text box text');
          equal(model.selected(), item, 'selected item');
        });

        test('do highlight', function () {
          var model = new Searchbox.viewModel();
          var item = { text: 'new result' };
          model.results.push('bar');
          model.results.push(item);
          model.results.push('foo');
          model.doHighlight(item, {});
          equal(model.highlighted(), 1, 'highlighted index');
        });

        squireDone();
      });
  });
});