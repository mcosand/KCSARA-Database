/*
 * Copyright 2015 Matthew Cosand
 */
define(['jquery', 'knockout', 'site/utils', 'site/env', 'moment', 'signalr-hubs'], function ($, ko, utils, env, moment) {
  env.registerComponent('event-quick-create', {
    viewModel: function (params) {
      var self = this;

      self.eventType = params.type;
      self.isForSignin = params.forSignin || false;
      this.onSuccess = params.onSuccess || function (theEvent) {
        utils.goToEvent(theEvent.id, theEvent.eventType, self.isForSignin ? 'signin' : 'detail');
      };

      this.title = ko.observable();
      this.start = ko.observable(utils.roundTime(moment(), 15));
      this.isActive = ko.observable(self.isForSignin);
      this.isActive.enabled = !self.isForSignin;


      var hub = $.connection.appHub;
      hub.client.eventUpdated = function (theEvent) {
        if (theEvent.eventType == self.eventType && !self.working())
        {
          require(['bootstrap-dialog'], function(dialog) {
            var buttons = [{
              label: 'View',
              cssClass: 'btn-default',
              action: function (me) { this.spin(); utils.goToEvent(theEvent.id, theEvent.eventType, 'detail') }
            },{
              label: 'Ignore',
              cssClass: self.isForSignin ? 'btn-default' : 'btn-primary',
              action: function (me) { me.close();}
            }];
            if (self.isForSignin) {
              buttons.splice(1, 0, {
                label: 'Sign In',
                cssClass: 'btn-primary',
                action: function (me) { this.spin(); utils.goToEvent(theEvent.id, theEvent.eventType, 'signin') }
              })
            }
            dialog.show({
              title: 'Updated ' + theEvent.eventType,
              message: 'Someone just updated a ' + theEvent.eventType.toLowerCase() + ' \'' + theEvent.title + '\'.<br/>' +
                'You can discard the one you\'re creating and use that one, or ignore and continue working.',
              draggable: true,
              spinicon: 'fa fa-circle-o-notch',
              buttons: buttons
            });
          })
        }
      }
      $.connection.hub.start({ waitForPageLoad: false });

      this.working = ko.observable(false);

      this._handleResponse = function (r) {
        if (r['errors'] && r['errors'].length > 0) {
          utils.applyErrors(self, r.errors);
          self.working(false);
          return;
        }
        self.onSuccess(r.data);
      };

      this.doSubmit = function (formElement) {
        var err = false;
        err = utils.testAndSetError(utils.checkNotNull, self.title, 'Required') || err;
        err = utils.testAndSetError(utils.checkNotNull, self.start, 'Required') || err;
        if (!err) {
          self.working(true);
          console.log(ko.toJS(self));
          utils.postJSON('/api/' + params.controller + '/update', ko.toJSON(self))
          .done(self._handleResponse)
          .fail(function (err) {
            utils.handleServiceError(err, self);
            self.working(false);
          })
        }
        return false;
      };

      utils.extendForErrors(this);
    },
    template: { require: 'text!' + window.appRoot + '/Components/Get/EventQuickCreate' }
  });
});