define(['testlib/squire'], function (Squire) {
  window.pushSquire(function (squireDone) {
    var mockQuery = {
      lastCall: null,
    };
    mockQuery.ajax = function (url, args) {
      mockQuery.lastCall = { url: url, args: args };
    }

    new Squire()
    .mock('jquery', mockQuery)
    .require(["site/utils"], function (utils) {
      module('site/utils');
      // ------------------------------------------------
      test("postJSON", function () {
        utils.postJSON('myUrl', { my: 'data' });
        equal(mockQuery.lastCall.url, 'myUrl', 'url is modified');
        equal(mockQuery.lastCall.args.type, 'POST', 'http verb');
      });
      // ------------------------------------------------
      test("getJSON", function () {
        utils.getJSON('getUrl', { my: 'get data' });
        equal(mockQuery.lastCall.url, 'getUrl', 'url is modified');
        equal(mockQuery.lastCall.args.type, 'GET', 'http verb');
      });
      squireDone();
    });
  });

  window.pushSquire(function(squireDone) {
    new Squire()
    .require(['site/utils', "knockout"], function (utils, ko) {
      module('site/utils');
      // ------------------------------------------------
      test("addKOLoader", function () {
        var model = {};
        model.s = ko.observable();
        model.i = ko.observable();
        utils.addKOLoader(model);
        equal(typeof model.load, 'function', "has load function");

        var data = { s: 'is a string', i: 42 };
        model.load(data);
        equal(model.s(), data.s, 'string');
        equal(model.i(), data.i, 'numeric');
      });

      // ------------------------------------------------
      test('extendForErrors', function () {
        var model = {
          staticProp: 'static value',
          func: function () { },
          obsPropA: ko.observable(),
          obsPropB: ko.observable({ some: 'object' })
        };
        utils.extendForErrors(model);
        ok(ko.isObservable(model.obsPropA.error), 'propA has error property');
        ok(ko.isObservable(model.obsPropB.error), 'propB has error property');
        ok(ko.isObservable(model.error), 'model has error property');
        equal(model.staticProp['error'], undefined, "doesn't modify non-observable properties");
        equal(model.func['error'], undefined, "doesn't modify functions");
      });

      // ------------------------------------------------
      test('testAndSetError', function () {
        var model = {
          prop: ko.observable(true),
        };
        utils.extendForErrors(model);
        var errMsg = 'my message';
        var checker = function (o) { return o; };
        utils.testAndSetError(checker, model.prop, errMsg);
        equal(model.prop.error(), errMsg, 'error message');

        model.prop(false);
        utils.testAndSetError(checker, model.prop, errMsg);
        equal(model.prop.error(), null, 'valid message');
      });
      squireDone();
    });
  });

  window.pushSquire(function(squireDone) {
    var mockEnv = {
      lastLog: null,
      location: { search: '?this=is&a=query' },
      cookie: 'firstCookie=HereWeAre; authInfo={"username":"admin","isAuthenticated":true}; endCookie=foofoo'
    };
    mockEnv.log = function (msg) { mockEnv.lastLog = msg; };

    var mockJQuery = {
      lastToast: null,
    }
    mockJQuery.toaster = function (toast) { mockJQuery.lastToast = toast; };

    new Squire()
    .mock('site/env', mockEnv)
    .mock('jquery', mockJQuery)
    .require(['site/utils', 'knockout'], function (utils, ko) {
      module('site/utils');
      // ------------------------------------------------
      test('query strings', function () {
        equal(utils.qs['a'], 'query', 'last query argument');
        equal(utils.qs['this'], 'is', 'first query argument');
        equal(utils.qs['query'], null, 'not a query argument');
      });

      // ------------------------------------------------
      test('handleServiceError - aborted', function () {
        mockJQuery.lastToast = null;
        mockEnv.lastLog = null;
        utils.handleServiceError({ status: 0 });
        equal(mockJQuery.lastToast, null, 'no toast');
        equal(mockEnv.lastLog, null, 'no log message');
      });

      // ------------------------------------------------
      test('handleServiceError - observable', function () {
        var model = {
          a: ko.observable(),
        };
        utils.extendForErrors(model);
        mockJQuery.lastToast = null;
        utils.handleServiceError({
          status: 400, responseJSON: {
            a: 'error a',
            total: 'global error'
          }
        }, model);

        equal(model.a.error(), 'error a', 'property error');
        equal(model.error(), 'global error\n', 'model errors');
        equal(null, mockJQuery.lastToast, 'no toast');
      });

      // ------------------------------------------------
      test('handleServiceError - unexpected', function () {
        mockJQuery.lastToast = null;
        mockEnv.lastLog = null;
        utils.handleServiceError({ status: 400 });
        notEqual(mockEnv.lastLog, null, 'log message');
        equal(mockJQuery.lastToast.priority, 'danger', 'toast priority');
      });

      // ------------------------------------------------
      test('getCookie - from environment', function () {
        equal(utils.getCookie('firstCookie'), "HereWeAre");
      })

      // ------------------------------------------------
      test('getCookie - not found', function () {
        equal(utils.getCookie('notFound', 'foo=blah;'), null);
      })

      // ------------------------------------------------
      test('getCookie - middle', function () {
        equal(utils.getCookie('authInfo'), '{"username":"admin","isAuthenticated":true}');
      })

      squireDone();
    });
  });
});