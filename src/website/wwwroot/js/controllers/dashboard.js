angular.module('sarDatabase').controller('DashboardCtrl', ['$scope', 'EventsService', function ($scope, EventsService) {
  $.extend($scope, {
    trainingStats: {},
    missionStats: {}
  });
  EventsService.stats($scope.missionStats, 'missions');
  EventsService.stats($scope.trainingStats, 'training');
  var map = L.map('eventMap').fitBounds([[47.16, -122.53], [47.77, -121.22]]);
  L.kcsarSetupMap(map, { showLayers: true, defaultBase: 'Google Streets' });
}]);