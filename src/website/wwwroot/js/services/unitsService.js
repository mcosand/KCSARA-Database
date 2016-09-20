angular.module('sar-database')

.provider('unitsService', function UnitsServiceProvider() {
  this.$get = ['$resource', function UnitsServiceFactory($resource) {
    var result = {
      units: $resource('/api2/units/:id'),
      members: $resource('/api2/units/:id/members')
    };

    return result;
  }]
});