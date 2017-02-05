angular.module('sar-database')

.provider('missionsService', function MissionsServiceProvider() {
  this.$get = ['$rootScope', '$http', 'Restangular', 'authService', function TrainingServiceFactory($rootScope,$http, Restangular) {
    var result = {
      missions: Restangular.service('missions'),
    };

    return result;
  }]
});