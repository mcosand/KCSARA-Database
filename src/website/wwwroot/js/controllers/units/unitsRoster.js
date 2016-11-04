angular.module('sar-database').controller("UnitsRosterCtrl", ['$stateParams', '$scope', 'unitsService', function ($stateParams, $scope, Units) {
  angular.extend($scope, {
    query: {
      order: 'name',
      limit: 5,
      page: 1
    },

    getRoster: function () {
      $scope.loading = Units.units.one($stateParams.id).all('memberships').getList().then(function (data) {
        $scope.roster = data;
      })
    },
    unit: {},
    roster: []
  });
  $scope.getRoster()
  Units.units.one($stateParams.id).get().then(function (unit) {
    $scope.unit = unit;
  })
}]);