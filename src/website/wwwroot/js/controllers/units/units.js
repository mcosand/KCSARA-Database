angular.module('sar-database').controller("UnitsCtrl", ['$scope', 'unitsService', function ($scope, unitsService) {
  angular.extend($scope, {
    units: []
  });
  unitsService.units.getList().then(function (list) { $scope.units = list });
}]);