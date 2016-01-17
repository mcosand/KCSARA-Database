angular.module('sarDatabase').factory('loginRecover', ['$q', '$injector', function($q, $injector) {
  var loginRecover = {
    responseError: function(response) {
      // Session has expired
      if (response.status == 403){
        var $uibModal = $injector.get('$uibModal');
        var $http = $injector.get('$http');
        var deferred = $q.defer();

        var modalInstance = $uibModal.open({
          templateUrl: window.appRoot + 'partials/login-modal.html',
          controller: 'LoginModalCtrl',
          controllerAs: 'modal',
          size: 'md',
          keyboard: false,
          backdrop: 'static'
        });
        modalInstance.result
        .then(function (data) {
          deferred.resolve(data);
        }, function (reason) {
          deferred.reject(reason);
        });

        // When the session recovered, make the same backend call again and chain the request
        var promise = deferred.promise;
        if (!response.config.dontRetryAfterLogin) {
          promise = promise.then(function() {
            return $http(response.config);
          });
        }
        return promise;
      }
      return $q.reject(response);
    }
  };
  return loginRecover;
}]);