angular.module('sar-database').controller("UnitsDetailCtrl", ['$stateParams', '$scope', 'unitsService', function ($stateParams, $scope, unitsService) {
  angular.extend($scope, {
    unit: {},
    statusTypes: {
      query: {
        order: 'name'
      },
      list: [],
      getList: function () {
        $scope.statusTypes.loading = unitsService.units.one($stateParams.id).all('statusTypes').getList().then(function (data) {
          $scope.statusTypes.list = data;
        })
      },
    }
  });
  $scope.statusTypes.getList();
  unitsService.units.one($stateParams.id).get().then(function (u) { $scope.unit = u; });
}]);