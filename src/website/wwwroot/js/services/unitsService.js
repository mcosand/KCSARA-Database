angular.module('sar-database')

.provider('unitsService', function UnitsServiceProvider() {
  this.$get = ['$rootScope', 'Restangular', 'authService', function UnitsServiceFactory($rootScope, Restangular) {
    var result = {
      units: Restangular.service('units'),
      createStatusType: function (unit) { return Restangular.restangularizeElement(null, { unit: unit, wacLevel: 'None' }, '/units/' + unit.id + '/statusTypes') },
      create: function () { return Restangular.restangularizeElement(null, { county: $rootScope.$defaultCounty }, '/units') }
    };

    return result;
  }]
});