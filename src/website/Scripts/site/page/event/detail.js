/*
 * Copyright 2015 Matthew Cosand
 */
define(['knockout', 'moment', 'site/utils'], function PageModel(ko, moment, utils) {
  return function (params) {
    var self = this;
    var paneResources = {
      roster: ['site/components/event-roster'],
      timeline: ['site/components/event-timeline'],
    };

    self.expanded = ko.observable(false);
    self.toggleExpand = function () {
      self.expanded(!self.expanded());
    }

    self.canEdit = true;
    self.doEdit = function () { }

    self.canDelete = true;
    self.doDelete = function () {
      require(['bootstrap-dialog'], function (dialog) {
        dialog.confirm({
          title: 'Delete Event',
          message: 'Are you sure you want to delete this event? There is no going back.',
          type: dialog.TYPE_DANGER,
          closable: true,
          draggable: true,
          autospin: true,
          spinicon: 'fa fa-circle-o-notch',
          btnOkLabel: 'Delete',
          callback: function (result) {
            if (!result) return;
            utils.postJSON('/api/' + self.controllerName + '/delete/' + self.eventId)
            .done(function (r) {
              if (r['errors'] && r['errors'].length > 0) {
                $.toaster({ title: 'Error', priority: 'danger', message: r.errors.map(function(el) { return el.text }).join('<br/>') });
              } else {
                window.location.href = window.appRoot + '/' + self.controllerName + '/list';
              }
            })
            .fail(function (err) { utils.handleServiceError(err, self); })
          }
        });
      });
    }

    self.eventId = $('body').data('eventId');
    self.controllerName = $('body').data('controllerName');

    self.activeTab = ko.observable();
    self.isOtherTab = ko.computed(function () { return self.activeTab() && self.activeTab().substring(0, 5) == "other" });
    self.topInfo = ko.observable();
    self.topInfo.loading = ko.observable(true);

    self.switchTab = function (newTab) {
      if (paneResources[newTab]) {
        require(paneResources[newTab], function () {
          self.activeTab(newTab);
        });
      } else {
        self.activeTab(newTab);
      }
    }

    self.pickTab = function (newTab, evt) {
      if (evt) newTab = $(evt.currentTarget).data('tab') || "";
      self.switchTab(newTab);
      window.location.hash = '#!' + newTab;
    }

    self.load = function loadEvents() {
      self.topInfo.loading(true);
      utils.getJSON('/api/' + self.controllerName + '/Overview/' + self.eventId)
      .done(function (data) {
        data.start = new moment(data.start);
        if (data.stop) data.stop = new moment(data.stop);
        self.topInfo(data);
      })
      .fail(function (err) { utils.handleServiceError(err, self); })
      .always(function () { self.topInfo.loading(false); });
    };


    var initialTab = utils.hashBangInit($('body').data('initialTab'), self.switchTab);
    self.switchTab(initialTab);
  };
});