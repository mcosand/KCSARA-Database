/*
 * Copyright 2015 Matthew Cosand
 */
angular.module('sarDatabase').service('EventsService', ['$http', '$q', function ($http, $q) {
  self = this;

  $.extend(this, {
    list: function (fillList, eventType, year) {
      var deferred = $q.defer();

      fillList.length = 0;
      fillList.loading = true;

      $http({
        method: 'GET',
        url: window.appRoot + 'api/' + eventType + '/list/' + (year || '')
      }).success(function (data) {
        $.each(data.events, function (idx, evt) {
          evt.date = moment(evt.date);
          fillList.unshift(evt);
        });
        fillList.people = data.people;
        fillList.miles = data.miles;
        fillList.hours = data.hours;
        delete fillList.loading;
        fillList.loaded = true;
        deferred.resolve(data);
      })
      .error(function (response) { deferred.reject(response); });
    },
    years: function (list, eventType) {
      var deferred = $q.defer();

      $http({
        method: 'GET',
        url: window.appRoot + 'api/' + eventType + '/years'
      }).success(function (data) {
        list.length = 0;
        list.push.apply(list, data);

        deferred.resolve(data);
      })
      .error(function (response) { deferred.reject(response); });
      return deferred.promise;
    }
  });
}]);