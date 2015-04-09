define(["site/app", "knockout", 'site/env'], function (AppModel, ko, env) {
  module('site/app')
  test("App is model with properties", function () {
    var model = new AppModel();
    notEqual(model, null, "instantiated");
    ok(ko.isObservable(model.searchResult), "searchResult is observable");
    notEqual(typeof model.page, 'undefined', "page property");
    ok(ko.isObservable(model.user), "user property");
  });

  test("env", function (assert) {
    env.log('this is a log message');
    var flag = false;
    var done = assert.async();
    env.setTimeout(function () { ok(true, 'fired timeout'); done(); }, 4);
  });
})