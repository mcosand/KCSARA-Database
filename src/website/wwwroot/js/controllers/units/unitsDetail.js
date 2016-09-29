angular.module('sar-database').controller("UnitsDetailCtrl", ['$stateParams', '$scope', 'editorsService', 'unitsService', function ($stateParams, $scope, Editors, Units) {
  angular.extend($scope, {
    unit: Units.units.one($stateParams.id).get().$object,
    statusTypes: {
      query: {
        order: 'name'
      },
      list: [],
      getList: function () {
        $scope.statusTypes.loading = Units.units.one($stateParams.id).all('statusTypes').getList().then(function (data) {
          $scope.statusTypes.list = data;
        })
      },
    },
    reports: {
      list: [],
      loading: true,
      getList: function() {
        Units.units.one($stateParams.id).all('reports').getList().then(function(data) {
          $scope.reports.list = data;
          delete $scope.reports.loading;
        })
      }
    },
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
  $scope.statusTypes.getList();
  $scope.reports.getList();
}]);