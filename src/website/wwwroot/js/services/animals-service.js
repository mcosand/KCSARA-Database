angular.module('sar-database')

.provider('animalsService', function AnimalsServiceProvider() {
  this.$get = ['$rootScope', '$http', 'Restangular', 'authService', function AnimalServiceFactory($rootScope, $http, Restangular) {
    var result = {
      animals: Restangular.service('animals'),
      create: function () { return Restangular.restangularizeElement(null, {}, '/animals') },
    };

    return result;
  }]
});