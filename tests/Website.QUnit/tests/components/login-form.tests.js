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
      .mock('knockout', ko)
      .mock('site/env', mockEnv)
      .require(["site/components/login-form", 'knockout'], function (login, testKO) {
        module('site/components/login-form');
        var LoginForm = mockEnv.registeredComponents['login-form'];
        // ------------------------------------------------
        test("is registered", function () {
          notEqual(typeof LoginForm, 'undefined');
        });

        // ------------------------------------------------
        test("view model init - defaults", function () {
          var model = new LoginForm.viewModel();
          equal(model.showLinks, undefined, 'show links');
          ok(testKO.isObservable(model.username), 'username observable');
          equal(model.username(), undefined, 'username initial state');
          ok(testKO.isObservable(model.password), 'password observable');
          equal(model.password(), undefined, 'password initial state');
          ok(testKO.isObservable(model.working), 'working observable');
          equal(model.working(), false, 'working initial state');
          ok(testKO.isObservable(model.error), 'error extended');
        });


        test("view model submit", function () {
          var model = new LoginForm.viewModel();
          model.username('test');
          model.password('password');
          model.doSubmit();
          equal(model.working(), true, 'working set');
          mockQuery.ajaxPromise.resolve({});
          equal(model.working(), false, 'working cleared');
        });

        squireDone();
      });
  });
});