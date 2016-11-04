angular.module('sar-database')

.provider('accountsService', function AccountsServiceProvider() {
  this.$get = ['$rootScope', '$http', 'Restangular', 'authService', function AccountsServiceFactory($rootScope, $http, Restangular) {
    var result = {
      create: function () { return Restangular.restangularizeElement(null, {}, '/accounts') },
      resetPassword: function (code, newPassword) {
        return $http.post('/api2/accounts/resetpassword', {code: code, newPassword: newPassword })
      },
      startResetPassword: function (username) {
        return $http.post('/api2/accounts/' + encodeURIComponent(username) + '/resetpassword');
      },
      accounts: Restangular.service('accounts'),
    };

    return result;
  }]
});