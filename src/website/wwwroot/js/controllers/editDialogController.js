angular.module('sar-database').controller('EditDialogCtrl', ['$mdDialog', '$scope', 'locals', function ($mdDialog, $scope, locals) {
  angular.extend($scope, locals, {
    cancel: function () { $mdDialog.cancel() },
    save: function () {
      $scope.saving = true;
      if (locals.saveMethod) {
        locals.saveMethod($scope, locals.item).then(function (data) {
          $scope.saving = false;
          $mdDialog.hide(data);
        }, function () {
          $scope.saving = false;
        })
      } else {
        $mdDialog.hide(locals.item);
      }
    }
  })
}])