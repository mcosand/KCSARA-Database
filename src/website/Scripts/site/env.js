define([], function () {
  var registered = {};
  return {
    log: function (a, b, c) { console.log(a, b, c) },
    setTimeout: function (fn, t) { window.setTimeout(fn, t) },
    clearTimeout: function (h) { window.clearTimeout(h) },
    cookie: document.cookie,
    location: window.location,
    registeredComponents: registered,
    registerComponent: function (name, data) { registered[name] = data; }
  }
});