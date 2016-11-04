angular.module('sar-database').controller("AccountFinishResetCtrl", ['$stateParams', '$state', '$scope', 'accountsService', function ($stateParams, $state, $scope, Accounts) {
  angular.extend($scope, {
    code: $stateParams.code,
    password: '',
    confirmPassword: '',

    saving: false,
    save: function (ev) {
      $scope.saving = true;
      Accounts.resetPassword($scope.code, $scope.password).then(function (data) {
        $scope.saving = false;
        $state.go('accounts_me');
      }).catch(function (err) {
        $scope.saving = false;
      })
    }
  });
}]);