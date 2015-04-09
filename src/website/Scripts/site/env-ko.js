define(['site/env', 'knockout'], function (env, ko) {
  var baseMethod = env.registerComponent;
  env.registerComponent = function (name, data) {
    baseMethod(name, data);
    ko.components.register(name, data);
  }
  return env;
});