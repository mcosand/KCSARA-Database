angular.module('sar-database')

.provider('membersService', function MembersServiceProvider() {
  this.$get = ['$rootScope', '$http', 'Restangular', 'editorsService', 'authService', function MembersServiceFactory($rootScope, $http, Restangular, Editors) {
    var result = {
      members: Restangular.service('members'),
      create: function () { return Restangular.restangularizeElement(null, {}, '/members') }
    };

    return result;
  }]
});