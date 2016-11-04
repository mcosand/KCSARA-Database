angular.module('sar-database').controller("AccountResetCtrl", ['$scope', 'accountsService', function ($scope, Accounts) {
  angular.extend($scope, {
    username: null,
    done: false,

    working: false,
    doReset: function (ev) {
      $scope.working = true;
      Accounts.startResetPassword($scope.username).then(function (data) {
        $scope.done = true;
      }).catch(function (err) {
        $scope.working = false;
      })
    }
  });
}]);