angular.module('sar-database').controller("AccountsCtrl", ['$state', '$scope', '$rootScope', 'editorsService', 'accountsService', function ($state, $scope, $rootScope, Editors, Accounts) {
  angular.extend($scope, {
    accounts: [],
    query: {
        order: 'username',
        limit: 5,
        page: 1
    },
    selected: [],

    getList: function () {
      $scope.loading = Accounts.accounts.getList().then(function (data) {
        $scope.accounts = data;
      })
    },
    gotoAccount: function (account) {
      $state.go('accounts_detail', { id: account.id });
    },
    createNew: function (ev) {
      var account = Accounts.create();
      Editors
        .doEditDialog(ev, '/wwwroot/partials/accounts/create.html', 'account', account)
        .then(function (newAccount) {
          $scope.accounts.push(newAccount);
        });
    }
  });
  $scope.getList();
}]);