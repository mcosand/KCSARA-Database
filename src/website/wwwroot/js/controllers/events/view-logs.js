define(['angular'], function(angular) {
  angular.module('sarDatabase').controller('EventLogCtrl', ['$scope', '$uibModal', 'EventsService',
  function ($scope, $uibModal, EventsService) {
    $.extend($scope, {
      logs: [],
      eventRoute: 'unknown',
      eventId: '',
      init: function(eventRoute, eventId) {
        $scope.eventRoute = eventRoute;
        $scope.eventId = eventId;
        EventsService.logs($scope.logs, eventRoute, eventId)
        .then(null, function (reason) { console.log(reason); alert('error'); });
      },
      dateText: function (date) {
        return date.calendar(null, {
          sameDay: '[Today] dddd MM/DD/YYYY',
          nextDay: '[Tomorrow]',
          nextWeek: 'dddd',
          lastDay: '[Yesterday] dddd MM/DD/YYYY',
          lastWeek: '[Last] dddd MM/DD/YYYY',
          sameElse: 'MM/DD/YYYY dddd'
        });
      },
      openLog: function(model, isCreate) {
        model.time.toJSON = function () { return model.time.format("YYYY-MM-DDTHH:mm") };
        model.timeText = function (val) {
          if (arguments.length && val) {
            model.time = moment(val, ["YYYY-MM-DD HH:mm", "YYYY-MM-DD HHmm", "MM/DD/YYYY HH:mm", "MM/DD/YYYY HHmm"]);
            model.time.toJSON = function () { return model.time.format("YYYY-MM-DDTHH:mm") };
            return model.time;
          } else if (arguments.length) {
            model.time = null;
          } else {
            return model.time.format('YYYY-MM-DD HH:mm');
          }
        };
        var modalInstance = $uibModal.open({
          templateUrl: window.appRoot + 'partials/events/edit-log.html',
          controller: 'CreateLogCtrl',
          controllerAs: 'modal',
          size: 'lg',
          resolve: {
            model: function() { return model },
            dialog: function() { return {
              eventRoute: '@ViewBag.EventRoute',
              isCreate: isCreate
            }}
          }
        });
        modalInstance.result
        .then(function (data) {
          EventsService.logs($scope.logs, '@ViewBag.EventRoute', '@Model.Id');
        }, function (reason) {
          // don't have to do anything.
        });
      },
      startCreate: function() {
        var model = {
          time: moment(),
          eventId: '@Model.Id'
        };
        $scope.openLog(model, true);
      },
      can: { create: true }
    });
  }]);
});