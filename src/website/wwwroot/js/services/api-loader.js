/*
 * Copyright 2016 Matthew Cosand
 */
angular.module('sarDatabase').service('ApiLoader', ['$http', '$q', function ($http, $q) {
  var self = this;
  var outstandingRequests = [];
  $.extend(this, {
    getInto: function (target, url, success, options) {
      var deferred = $q.defer();

      var previous = null;
      for (var i = 0; i < outstandingRequests.length; i++) {
        if (outstandingRequests[i].target === target) {
          console.log('cancelling previous request to ' + outstandingRequests[i].url);
          outstandingRequests[i].cancel.reject('replaced');
          outstandingRequests.splice(i, 1);
        }
      }

      var canceller = $q.defer();

      target.loading = true;
      var cancelInfo = { target: target, url: url, cancel: canceller };
      outstandingRequests.push(cancelInfo);

      $http({
        method: (options && options.method) ? options.method : 'GET',
        url: url,
        timeout: canceller.promise,
        data: (options && options.data) ? options.data : null
      }).success(function (data) {
        if (success) { success(data); }
        else {
          $.extend(target, data);
        }

        delete target.loading;
        target.loaded = true;
        outstandingRequests.splice(outstandingRequests.indexOf(cancelInfo), 1);
        deferred.resolve(data);
      })
      .error(function (response) {
        outstandingRequests.splice(outstandingRequests.indexOf(cancelInfo), 1);
        deferred.reject(response);
      });

      return deferred.promise;
    },
    getIntoList: function (list, url, success) {
      list.length = 0;
      return self.getInto(list, url, success || function (data) { list.push.apply(list, data); });
    }
  });
}]);