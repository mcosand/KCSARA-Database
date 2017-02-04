angular.module('sar-database').controller("UnitsDetailCtrl", ['$stateParams', '$scope', 'editorsService', 'listService', 'unitsService', function ($stateParams, $scope, Editors, Lists, Units) {
  angular.extend($scope, {
    unit: Units.units.one($stateParams.id).get().$object,
    statusTypes: Lists.loader(Units.units.one($stateParams.id).all('statusTypes'), { order: 'name' }),
    reports: Lists.loader(Units.units.one($stateParams.id).all('reports')),
    createNew: function(ev) {
      var status = Units.createStatusType($scope.unit);
      $scope.editStatus(ev, status);
    },
    editStatus: function (ev, status) {
      Editors
        .doEditDialog(ev, '/wwwroot/partials/units/editstatus.html', 'unit status', status, { wacLevels: ['None', 'Novice', 'Support', 'Field'] })
        .then(function () { $scope.statusTypes.getList(); });
    },
    deleteStatus: function (ev, status) {
      Editors
        .doDelete(ev, 'unit status', status)
        .then(function () { $scope.statusTypes.getList(); });
    }
  });
}]);