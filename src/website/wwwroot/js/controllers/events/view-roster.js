define(['angular'/*, 'controllers/events/edit-roster'*/], function (angular) {
  angular.module('sarDatabase').controller('EventRosterCtrl', ['$scope', '$uibModal', 'EventsService',
    function ($scope, $uibModal, EventsService) {
      $.extend($scope, {
        roster: [],
        current: null,
        currentDetails: null,
        eventRoute: 'unknown',
        eventId: '',
        init: function(eventRoute, eventId) {
          $scope.eventRoute = eventRoute;
          $scope.eventId = eventId;
          EventsService.roster($scope.roster, eventRoute, eventId)
          .then(null, function (reason) { console.log(reason); alert('error'); });
        },
        //editResponder: function(item) {
        //  $scope.openResponder(item, false);
        //},
        setCurrent: function(newValue) {
          if (newValue && $scope.current && newValue.id == $scope.current.id) {
            $scope.current = null;
            return;
          }

          $scope.current = $.extend({}, newValue, {
            timeline: []
          });
          EventsService.participantTimeline($scope.current.timeline, $scope.eventRoute, $scope.eventId, newValue.id);
        },
        startUpdateStatus: function(entry) {
          alert('ok');
        },
        //startCreate: function() {
        //  var model = {
        //    eventId: $scope.eventId,
        //    type: 'unknown'
        //  };
        //  $scope.openDoc(model, true);
        //},
        //openDoc: function(model, isCreate) {
        //  var modalInstance = $uibModal.open({
        //    templateUrl: window.appRoot + 'partials/events/edit-document.html',
        //    controller: 'CreateDocCtrl',
        //    controllerAs: 'modal',
        //    size: 'md',
        //    resolve: {
        //      model: function() { return model },
        //      dialog: function() { return {
        //        eventRoute: $scope.eventRoute,
        //        isCreate: isCreate
        //      }}
        //    }
        //  });
        //  modalInstance.result
        //  .then(function () {
        //    EventsService.documents($scope.documents, $scope.eventRoute, $scope.eventId);
        //  }, function (reason) {
        //    // don't have to do anything.
        //  });
        //},
        can: { create: true }
      });
    }]);
});