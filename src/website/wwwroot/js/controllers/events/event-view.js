angular.module('sarDatabase').controller('EventViewCtrl', ['$scope', '$location', '$uibModal', 'EventsService',
function ($scope, $location, $uibModal, EventsService) {
  function parseTabFromhash(value) {
    return (value.match(/^#\/?([^\/]+)/) || ['#/info', 'info'])[1];
  };
  $.extend($scope, {
    activeTab: parseTabFromhash($location.path()),
    setTab: function (newTab) {
      $location.path(newTab);
    }
  });
  $scope.$watch(function () { return location.hash }, function (value) { $scope.activeTab = parseTabFromhash(value); });
}]);
