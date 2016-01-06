/*
 * Copyright 2015 Matthew Cosand
 */
define(['moment', 'sarDatabase'], function (moment) {
  angular.module('sarDatabase').service('EventsService', ['$http', '$q', function ($http, $q) {
    self = this;
    var outstandingRequests = [];

    function getIntoList(list, url, success) {
      var deferred = $q.defer();

      var previous = null;
      for (var i = 0; i < outstandingRequests.length; i++) {
        if (outstandingRequests[i].list === list) {
          console.log('cancelling previous request to ' + outstandingRequests[i].url);
          outstandingRequests[i].cancel.reject('replaced');
          outstandingRequests.splice(i, 1);
        }
      }

      var canceller = $q.defer();

      list.length = 0;
      list.loading = true;
      var cancelInfo = { list: list, url: url, cancel: canceller };
      outstandingRequests.push(cancelInfo);

      $http({
        method: 'GET',
        url: url,
        timeout: canceller.promise
      }).success(function (data) {
        if (success) { success(data); }
        else {
          list.push.apply(list, data);
        }

        delete list.loading;
        list.loaded = true;
        outstandingRequests.splice(outstandingRequests.indexOf(cancelInfo), 1);
        deferred.resolve(data);
      })
      .error(function (response) {
        outstandingRequests.splice(outstandingRequests.indexOf(cancelInfo), 1);
        deferred.reject(response);
      });

      return deferred.promise;
    }

    $.extend(this, {
      list: function (fillList, eventType, year) {
        return getIntoList(
          fillList,
          window.appRoot + 'api/' + eventType + '/list/' + (year || ''),
          function (data) {
            $.each(data.events, function (idx, evt) {
              evt.date = moment(evt.date);
              fillList.unshift(evt);
            });
            fillList.people = data.people;
            fillList.miles = data.miles;
            fillList.hours = data.hours;
          })
      },
      years: function (list, eventType) {
        return getIntoList(
          list,
          window.appRoot + 'api/' + eventType + '/years');
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
      documents: function (fillList, eventType, eventId) {
        return getIntoList(
          fillList,
          window.appRoot + 'api/documents/' + eventId,
          function (data) {
            $.each(data, function (idx, doc) {
              doc.thumbUrl = window.appRoot + ((doc.mime.substring(0, 5) == "image") ? ('documents/' + doc.id + '/thumbnail') : ('images/mime/' + doc.mime.replace('/', '_') + '.png'));
              doc.url = window.appRoot + 'documents/' + doc.id;
              doc.size = Math.round((doc.size / 1024) * 10) / 10 + 'KB';
              fillList.push(doc);
            })
          });
      },
      logs: function (fillList, eventType, eventId) {
        return getIntoList(
          fillList,
          window.appRoot + 'api/' + eventType + '/' + eventId + '/logs',
          function(data) {
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
          });
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
        .error(function (response, statusCode) { deferred.reject(statusCode == 403 ? 'login' : response); });
        return deferred.promise;
      },
    });
  }]);
});