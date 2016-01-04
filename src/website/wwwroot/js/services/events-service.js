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
    },
    save: function (model, type) {
      var deferred = $q.defer();
      //if (model.linkedMember.loaded) delete model.linkedMember.loaded;
      $http({
        method: model['id'] ? 'PUT' : 'POST',
        url: window.appRoot + 'api/' + type,
        data: model,
      })
      .success(function (data) {
        data.start = moment(data.start);
        if (data.stop) { data.stop = moment(data.stop); }
        deferred.resolve(data);
      })
      .error(function (response) { deferred.reject(response); })
      return deferred.promise;
    },
    logs: function (fillList, eventType, eventId) {
      var deferred = $q.defer();

      fillList.length = 0;
      fillList.loading = true;

      $http({
        method: 'GET',
        url: window.appRoot + 'api/' + eventType + '/' + eventId + '/logs'
      }).success(function (data) {
        $.each(data, function (idx, evt) {
          evt.eventId = eventId;
          evt.time = moment(evt.time);
          var day = moment(evt.time);
          day.startOf('day');
          if (fillList.length == 0 || fillList[fillList.length - 1].date.format() != day.format()) {
            fillList.push({ date: day, logs: [] });
          }
          fillList[fillList.length - 1].logs.push(evt);
        });
        delete fillList.loading;
        fillList.loaded = true;
        deferred.resolve(data);
      })
      .error(function (response) { deferred.reject(response); });
    },
    saveLog: function (model, type) {
      var deferred = $q.defer();
      //if (model.linkedMember.loaded) delete model.linkedMember.loaded;
      $http({
        method: model['id'] ? 'PUT' : 'POST',
        url: window.appRoot + 'api/' + type + '/' + model.eventId + '/log',
        data: model,
      })
      .success(function (data) {
        data.time = moment(data.time);
        deferred.resolve(data);
      })
      .error(function (response) { deferred.reject(response); })
      return deferred.promise;
    },
  });
}]);