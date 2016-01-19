/*
 * Copyright 2016 Matthew Cosand
 */
angular.module('sarDatabase').service('MembersService', ['$http', '$q', function ($http, $q) {
  var self = this;
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
    addresses: function (fillList, memberId) {
      return getIntoList(
        fillList,
        window.appRoot + 'api/members/' + memberId + '/addresses',
        null)
    },
    contacts: function (fillList, memberId) {
      return getIntoList(
        fillList,
        window.appRoot + 'api/members/' + memberId + '/contacts',
        null)
    }
  });
}]);