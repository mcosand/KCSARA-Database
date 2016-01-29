/*
 * Copyright 2016 Matthew Cosand
 */
angular.module('sarDatabase').service('UnitsService', ['ApiLoader', '$http', '$q', function (ApiLoader, $http, $q) {
  $.extend(this, {
    roster: function (fillList, unitId) {
      return ApiLoader.getIntoList(
        fillList,
        window.appRoot + 'api/units/' + unitId + '/roster',
        function (data) {
          $.each(data, function (idx, m) {
            m.asOf = moment(m.asOf);
            fillList.push(m);
          });
        });
    }
  });
}]);