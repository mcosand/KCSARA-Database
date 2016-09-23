angular.module('sar-database').controller("UnitsDetailCtrl", ['$stateParams', '$scope', '$mdDialog', 'unitsService', function ($stateParams, $scope, $mdDialog, unitsService) {
  function doEditor(ev, status) {
    $mdDialog.show({
      controller: 'EditDialogCtrl',
      //        bindToController: true,
      locals: {
        titlePrefix: status.id ? 'Edit "' + status.name + '"' : "Add New",
        item: status.plain(), wacLevels: ['None', 'Novice', 'Support', 'Field'], saveMethod: function (newValues) {
          angular.extend(status, newValues);
          return status.save().then(function (result) {
            console.log({ action: 'back from saving status', result: result })
            return result;
          })
        }
      },
      targetEvent: ev,
      templateUrl: '/wwwroot/partials/units/editstatus.html'
    })
    .then(function (statusValues) {
      $scope.statusTypes.getList();
    })
    .catch(function () {
      console.log('cancelled. Do nothing');
    })
  }

  angular.extend($scope, {
    unit: unitsService.units.one($stateParams.id).get().$object,
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
    },
    createNew: function(ev) {
      var status = unitsService.createStatusType($scope.unit);
      doEditor(ev, status);
    },
    editStatus: function (ev, status) {
      console.log('Hi Hi')
      console.log(status);
      doEditor(ev, status);
    },
    deleteStatus: function(ev, status) {
      var confirm = $mdDialog.confirm()
      .title('Delete Status')
      .textContent('Are you sure you want to delete status "' + status.name + '"?')
      .ariaLabel('Delete Status ' + status.name)
      .targetEvent(ev)
      .ok('Delete')
      .cancel('Cancel');
      $mdDialog.show(confirm).then(function () {
        console.log(status);
        //status.remove().then(function () {
        //  $scope.statusTypes.getList();
        //})
        //.catch(function () {
        //  alert('ERROR REMOVING STATUS')
        //})
      })
    }
  });
  $scope.statusTypes.getList();
}]);