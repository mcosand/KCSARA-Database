define(['jquery', 'knockout', 'site/env', 'moment'], function ($, ko, env, moment) {
  // Here's a custom Knockout binding that makes elements shown/hidden via jQuery's fadeIn()/fadeOut() methods
  // Could be stored in a separate utility library
  ko.bindingHandlers.fadeVisible = {
    init: function (element, valueAccessor) {
      // Initially set the element to be instantly visible/hidden depending on the value
      var value = valueAccessor();
      $(element).toggle(ko.unwrap(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor) {
      // Whenever the value subsequently changes, slowly fade the element in or out
      var value = valueAccessor();
      ko.unwrap(value) ? $(element).fadeIn() : $(element).hide();
    }
  };

  ko.bindingHandlers.nbText = {
    update: function (element, valueAccessor) {
      var value = valueAccessor();
      var el = $(element);
      el.text(ko.unwrap(value));
      el.html(el.html().replace(/\-/g, "&#8209;").replace(" ", "&nbsp;"));
    }
  }

  ko.bindingHandlers.sort = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
      var asc = false;
      element.style.cursor = 'pointer';

      element.onclick = function () {
        var value = valueAccessor();
        var prop = value.prop;
        var data = value.arr;

        asc = !asc;

        data.sort(function (left, right) {
          var rec1 = left;
          var rec2 = right;

          if (!asc) {
            rec1 = right;
            rec2 = left;
          }

          var props = prop.split('.');
          for (var i in props) {
            var propName = props[i];
            var parenIndex = propName.indexOf('()');
            if (parenIndex > 0) {
              propName = propName.substring(0, parenIndex);
              rec1 = rec1[propName]();
              rec2 = rec2[propName]();
            } else {
              rec1 = rec1[propName];
              rec2 = rec2[propName];
            }
          }

          return rec1 == rec2 ? 0 : rec1 < rec2 ? -1 : 1;
        });
      };
    }
  };

  ko.bindingHandlers.foreachWithHighlight = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, context) {
      var flashReset = valueAccessor().initReset,
        key = "forEachWithHightlight_initialized";
      if (flashReset) {
        flashReset.subscribe(function () { ko.utils.domData.set(element, key, false) });
      }
      return ko.bindingHandlers.foreach.init(element, valueAccessor, allBindingsAccessor, viewModel, context);
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, context) {
      var args = valueAccessor();
      var value = ko.unwrap(args.data),
          duration = ko.unwrap(args.duration) || 800,
          key = "forEachWithHightlight_initialized";

      var newValue = function () {
        return {
          data: value,
          afterAdd: function (el, index, data) {
            if (ko.utils.domData.get(element, key)) {
              $(el).addClass('info');
              window.setTimeout(function () { $(el).removeClass('info') }, duration);
            };
          }
        };
      };

      ko.bindingHandlers.foreach.update(element, newValue, allBindingsAccessor, viewModel, context);

      //if we have not previously marked this as initialized and there are currently items in the array, then cache on the element that it has been initialized
      if (!ko.utils.domData.get(element, key) && value.length) {
        ko.utils.domData.set(element, key, true);
      }

      return { controlsDescendantBindings: true };
    }
  };
  ko.observableArray.fn.replaceById = function (id, newValue) {
    this.valueWillMutate();
    var arr = this();
    var found = false;
    for (var i = 0; i < arr.length; i++)
    {
      if (arr[i].id == id) { arr[i] = newValue; found = true; }
    }
    this.valueHasMutated();
    return found;
  }

  var utils = {};
  utils.hashBangInit = function hashBangInit(initialPage, callback) {
    $(window).on('hashchange', function () {
      console.log("hash changed: ")
      if (window.location.hash.length < 2 || window.location.hash == '#!') {
        callback(initialPage);
      } else if (window.location.hash.length > 1 && window.location.hash[1] == '!') {
        callback(window.location.hash.substring(2));
      }
    });

    return (window.location.hash.length > 1 && window.location.hash[1] == '!') ? window.location.hash.substring(2) : initialPage;
  };

  utils.getCookie = function getCookie(name, source) {
    var src = source || env.cookie;
    if (!src) return null;
    var searched = src.split(';').map(function (e) { return e.trim() }).filter(function (s) {
      return s.search(name + '=') == 0
    })
    if (searched.length == 0) return undefined;
    return searched[0].split(/(=)/).slice(2).join('')
  };

  utils.addKOLoader = function addKOLoader(obs) {
    obs.load = function loader(data) {
      if (data == null) return;
      for (var p in obs) {
        if (ko.isObservable(obs[p]) && data[p] !== undefined) {
          if (/*object is simple*/true) { obs[p](data[p]); }
        }
      }
      return obs;
    }
  };

  utils.debounce = function debounce(func, wait, immediate, stateFunc) {
    var timeout;
    var previousState = null;
    return function () {
      var context = this, args = arguments;
      var later = function () {
        timeout = null;
        if (stateFunc) {
          var now = stateFunc();
          if (previousState != now) { previousState = now; func.apply(context, args); }
        } else if (!immediate) {
          func.apply(context, args);
        }
      };
      var callNow = immediate && !timeout;
      env.clearTimeout(timeout);
      timeout = env.setTimeout(later, wait);
      if (callNow) {
        if (stateFunc) previousState = stateFunc();
        func.apply(context, args);
      }
    };
  };

  utils.postJSON = function postJSON(relativeUrl, data) {
    return $.ajax('' + relativeUrl, { type: 'POST', contentType: 'application/json', dataType: 'json', data: data });
  };
  utils.getJSON = function getJSON(relativeUrl, data) {
    return $.ajax('' + relativeUrl, { type: 'GET', contentType: 'application/json', dataType: 'json', data: data });
  };

  utils._loginDialog = function (onSuccess) {
    require(['bootstrap-dialog'], function (BootstrapDialog) {
      var dialog;
      var localModel = {
        onSuccess: function () { onSuccess(dialog); }
      };
      dialog = BootstrapDialog.show({
        closable: false,
        title: 'Login',
        message: '<p>Please login in again:</p><login-form params="onSuccess: onSuccess"></login-form>',
        onshown: function (a) {
          ko.applyBindings(localModel, a.getModalBody()[0]);
        }
      });
    });
  }


  /* getJSONRetriable
   * If a get request fails for some reasons (auth timeout), retry it.
   * If the reason is an auth expiration, re-authenticate the user and then retry the request.
   *
   * Re-auth: Use a wrapping deferred object. Load bootstrapdialog and show the login form. The dialog
   * can't be closed so the user has to post an auth request (or leave the page). When we get back from
   * a successful auth request we re-issue the request.
   *
   * When we get to a success or unhandled failed state, resolve or reject the wrapping deferred respectively.
   */
  utils._reauth = { working: false, requests: [] };
  utils.getJSONRetriable = function getJSONRetriable(relativeUrl, data) {
    var dfrd = $.Deferred(),
      promise = dfrd.promise(),
      jqXHR;

    promise.go = function () {
      if (!jqXHR) {
        jqXHR = utils.getJSON(relativeUrl, data)
          .fail(function (err) {
            if (err.status == 401) {
              // todo - wrap this in another deferred?
              utils._reauth.requests.push(promise.go);
              if (utils._reauth.working == false) {
                utils._reauth.working = true;
                utils._loginDialog(function (dialog) {
                  jqXHR = false;
                  dialog.close();
                  window.setTimeout(function () {
                    console.log('running ' + utils._reauth.requests.length + " requests");
                    utils._reauth.working = false;
                    for (var i = 0; i < utils._reauth.requests.length; i++) {
                      utils._reauth.requests[i]();
                    }
                    utils._reauth.requests.length = 0;
                  }, 10);
                });
              }
            }
            else { dfrd.reject(err); }
          })
        .done(function (data) { dfrd.resolve(data) });
      }
      return promise;
    }
    promise.abort = function abort() {
      if (jqXHR) {
        jqXHR.abort();
        dfrd.reject({ status: 0 });
      }
    }
    return promise;
  }

  utils.log = function (message) {
    env.log(message);
  };

  utils.applyErrors = function applyErrors(model, errs) {
    for (var i = 0; i < errs.length; i++) {

      if (errs[i].property === undefined || errs[i].property === null || errs[i].property === '') {
        model.error(errs[i].text);
      }
      else {
        model[errs[i]].error(errs[i].text);
      }
    }
  };

  utils.handleServiceError = function handleServiceError(err, model) {
    if (err.status == 0) return;
    if (err.status == 401) {
      // If you want to get the user to re-authenticate and retry the request, use utils.getJSONRetriable
      $.toaster({ title: 'Error', priority: 'danger', message: 'Permission denied' })
    } else {
      utils.log(err);
      if (err.responseJSON) {
        if (model && ko.isObservable(model.error)) {
          var val = "";
          for (var p in err.responseJSON) {
            if (model[p] && ko.isObservable(model[p]['error'])) {
              model[p].error(err.responseJSON[p]);
            } else {
              val += err.responseJSON[p] + "\n";
            }
          }
          model.error(val);
          return;
        }
      }
      $.toaster({ title: 'Error', priority: 'danger', message: 'An error occured talking to server' })
    }
  };

  utils.testAndSetError = function testAndSetError(checker, observable, message) {
    observable.error(null);
    if (checker(observable())) {
      observable.error(message);
      return true;
    }
    return false;
  };

  utils.checkNotNull = function checkNotNull(value) { return value == null; };

  utils.extendForErrors = function extendForErrors(model) {
    if (model == null) return;
    for (var p in model) {
      if (ko.isObservable(model[p]) && model[p]['error'] === undefined) { model[p].error = ko.observable(); }
    }
    if (model.error === undefined) model.error = ko.observable();
  };

  var searchParts = env.location.search.substr(1).split('&');
  utils.qs = (function (a) {
    if (a == "") return {};
    var b = {};
    for (var i = 0; i < a.length; ++i) {
      var p = a[i].split('=', 2);
      if (p.length == 1)
        b[p[0].toLowerCase()] = "";
      else
        b[p[0].toLowerCase()] = decodeURIComponent(p[1].replace(/\+/g, " "));
    }
    return b;
  })(searchParts);

  utils.goToEvent = function (id, type, page, subpage) {
    var controller = type == 'Mission' ? 'Missions' : type;
    var url = [window.appRoot, controller, page, id].join('/');
    if (subpage) url += '#!' + subpage;
    window.location.href = url;
  }

  utils.roundTime = function(m, roundMinutes)
  {
    var rounding = roundMinutes * 60 * 1000; /*ms*/
    return moment(Math.floor((+m) / rounding) * rounding);
  }

  return utils;
});