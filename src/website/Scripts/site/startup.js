﻿define(['jquery', 'knockout', 'site/utils', 'site/app', 'bootstrap', 'jquery.toaster'], function ($, ko, utils, AppModel) {
  var authInfo = JSON.parse(utils.getCookie("authInfo") || "{}");
  if (authInfo) {
    document.cookie = 'authInfo=;expires=Thu, 01 Jan 1970 00:00:01 GMT;';
  }

  var _resizeGlued = function () {
    var el = $(this);
    var anchor = el.data('anchorNode');
    if (anchor == null) { _setupGlued(el); }
    var offset = anchor.offset();
    el.css({ 'position': 'absolute', 'left': offset.left + 'px', 'top': (offset.top + anchor.outerHeight()) + 'px', 'width': Math.max(anchor.outerWidth(), 200) + "px", 'z-index': 1040 });
  }
  var _setupGlued = function (element) {
    var el = element || $(this);
    el.data('anchorNode', el.prev());
    $(document.body).append(el.detach());
  }

  var model = new AppModel();
  model.user().load(authInfo);
  model.page = pageModel;
  ko.applyBindings(model);
  $(window).on('resize', function () { $('.glue-previous').each(_resizeGlued) });
  window.setTimeout(function () { $('.glue-previous').each(_setupGlued); $(window).trigger('resize') }, 200);
  
  $.toaster({ settings: { timeout: 3000, donotdismiss: ['danger'] } });

  window.onerror = function (err, msg, loc) {
    var packet = { Error: err, Message: msg, Location: loc };
    utils.log(packet);
    utils.postJSON("/api/telemetry/error", JSON.stringify(packet));
  };

  console.info("app started");
});