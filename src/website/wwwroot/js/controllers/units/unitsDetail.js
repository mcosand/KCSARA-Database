angular.module('sar-database').controller("UnitsDetailCtrl", ['$stateParams', '$scope', '$rootScope', 'Restangular', 'unitsService', function ($stateParams, $scope, $rootScope, Restangular, unitsService) {
  angular.extend($scope, {
    unit: unitsService.units.get({ id: $stateParams.id }, function () {
      $rootScope.$title = $scope.unit.name;
    }),
    statusTypes: {
      query: {
        order: 'name'
      },
      list: [],
      getList: function () {
        $scope.statusTypes.loading = Restangular.one('api2/units', $stateParams.id).all('statusTypes').getList().then(function (data) {
          $scope.statusTypes.list = data;
        })
      },
    }
  });
  $scope.statusTypes.getList();
}]);