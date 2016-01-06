define(['angular', 'controllers/events/create-document'], function (angular) {
  angular.module('sarDatabase').controller('EventDocsCtrl', ['$scope', '$uibModal', 'EventsService',
    function ($scope, $uibModal, EventsService) {
      $.extend($scope, {
        documents: [],
        eventRoute: 'unknown',
        eventId: '',
        init: function(eventRoute, eventId) {
          $scope.eventRoute = eventRoute;
          $scope.eventId = eventId;
          EventsService.documents($scope.documents, eventRoute, eventId)
          .then(null, function (reason) { console.log(reason); alert('error'); });
        },
        gotoDoc: function(url) {
          window.open(url, '_blank', '');
        },
        startCreate: function() {
          var model = {
            eventId: $scope.eventId,
            type: 'unknown'
          };
          $scope.openDoc(model, true);
        },
        openDoc: function(model, isCreate) {
          var modalInstance = $uibModal.open({
            templateUrl: window.appRoot + 'partials/events/edit-document.html',
            controller: 'CreateDocCtrl',
            controllerAs: 'modal',
            size: 'md',
            resolve: {
              model: function() { return model },
              dialog: function() { return {
                eventRoute: $scope.eventRoute,
                isCreate: isCreate
              }}
            }
          });
          modalInstance.result
          .then(function (data) {
            EventsService.documents($scope.documents, $scope.eventRoute, $scope.eventId);
          }, function (reason) {
            // don't have to do anything.
          });
        },
        can: { create: true }
      });
    }]);
});