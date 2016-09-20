angular.module('sar-database').controller("UnitsRosterCtrl", ['$stateParams', '$scope', '$rootScope', '$timeout', 'unitsService', 'Restangular', function ($stateParams, $scope, $rootScope, $timeout, unitsService, Restangular) {
  angular.extend($scope, {
    query: {
      order: 'name',
      limit: 5,
      page: 1
    },

    getRoster: function () {
      $scope.loading = Restangular.one('api2/units', $stateParams.id).all('memberships').getList().then(function (data) {
        $scope.roster = data;
      })
    },

    unit: unitsService.units.get({ id: $stateParams.id }, function () {
      $rootScope.$title = $scope.unit.name;
      // $timeout(function () { $rootScope.title = $scope.unit.name; });
    })
  });
  $scope.getRoster()
}]);