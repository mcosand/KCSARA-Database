angular.module('sar-database')

.provider('unitsService', function UnitsServiceProvider() {
  this.$get = ['Restangular', 'authService', function UnitsServiceFactory(Restangular) {
    var result = {
      units: Restangular.service('units')
    };

    return result;
  }]
});