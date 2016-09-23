angular.module('sar-database')

.provider('unitsService', function UnitsServiceProvider() {
  this.$get = ['Restangular', 'authService', function UnitsServiceFactory(Restangular) {
    var result = {
      units: Restangular.service('units'),
      createStatusType: function (unit) { return Restangular.restangularizeElement(null, { unit: unit, wacLevel: 'None' }, '/units/' + unit.id + '/statusTypes')}
    };

    return result;
  }]
});