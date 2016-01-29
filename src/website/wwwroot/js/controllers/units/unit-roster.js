angular.module('sarDatabase').controller('UnitRosterCtrl', ['$scope', 'UnitsService',
  function ($scope, UnitsService) {
    $.extend($scope, {
      roster: [],
      unitId: null,
      options: { showAll: false },
      init: function (unitId) {
        $scope.unitId = unitId;
        $scope.sort('member.name');
        UnitsService.roster($scope.roster, unitId)
        .then(null, function (reason) { console.log(reason); alert('error'); });
      },
      sort: function (column) {
        if (column == $scope.roster.sort) {
          $scope.roster.sortDir = !$scope.roster.sortDir;
        } else {
          $scope.roster.sort = column;
          $scope.roster.sortDir = false;
        }
      },
      rosterFilter: function (item) {
        return $scope.options.showAll === "true" || item.isActive;
      }
    });
  }]);