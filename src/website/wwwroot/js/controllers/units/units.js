angular.module('sar-database').controller("UnitsCtrl", ['$scope', '$rootScope', 'unitsService', 'editorsService', function ($scope, $rootScope, Units, Editors) {
  refresh = function Units_Refresh() {
    Units.units.getList().then(function (list) { $scope.units = list });
  }

  angular.extend($scope, {
    units: [],
    createNew: function (ev) {
      var unit = Units.create();
      $scope.editUnit(ev, unit);
    },
    editUnit: function (ev, unit) {
      Editors
        .doEditDialog(ev, '/wwwroot/partials/units/edit.html', 'unit', unit, { counties: [ $rootScope.$defaultCounty ] })
        .then(function () { refresh(); });
    },
    deleteUnit: function (ev, unit) {
      Editors
        .doDelete(ev, 'unit', unit)
        .then(function () { refresh(); });
    },
  });
  refresh();
}]);