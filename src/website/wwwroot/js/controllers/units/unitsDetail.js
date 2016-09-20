angular.module('sar-database').controller("UnitsDetailCtrl", ['$stateParams', '$scope', '$rootScope', '$timeout', 'unitsService', function ($stateParams, $scope, $rootScope, $timeout, unitsService) {
  angular.extend($scope, {
    unit: unitsService.units.get({ id: $stateParams.id }, function () {
      $rootScope.$title = $scope.unit.name;
    })
  });
}]);