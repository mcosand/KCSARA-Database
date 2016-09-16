angular.module('sarAuth')
.controller('RegisterLoginCtrl', ['$scope', '$http', '$window', function ($scope, $http, $window) {
  angular.extend($scope, {
    init: function (email) {
      $scope.model = { email: email };
    },

    sending: false,
    codeSent: false,
    verifying: false,
    serverErrors: [],

    sendCode: function () {
      if ($scope.emailForm.$invalid) {
        return;
      }

      $scope.sending = true;
      $scope.codeSent = false;

      $http({
        method: 'POST',
        url: window.appRoot + 'auth/externalVerificationCode',
        data: { email: $scope.model.email }
      }).then(function (resp) {
        if (resp.data.success) { $scope.codeSent = true; }
        else {
          angular.forEach(resp.data.errors, function(errors, field) {
            $scope.emailForm[field].$setValidity('server', false)
            $scope.serverErrors[field] = errors.join(', ')
          })
        }
      }, function (resp) {
        alert('Error: ' + resp.data);
      })['finally'](function () {
        $scope.sending = false;
      })
    },
    verify: function () {
      $scope.verifying = true;
      $http({
        method: 'POST',
        url: window.appRoot + 'auth/verifyExternalCode',
        data: { code: $scope.model.code, email: $scope.model.email }
      }).then(function (resp) {
        if (resp.data.success) { $window.location.href = resp.data.url; }
        else {
          angular.forEach(resp.data.errors, function (errors, field) {
            $scope.verifyForm[field].$setValidity('server', false)
            $scope.serverErrors[field] = errors.join(', ')
          })
        }
      }, function (resp) {
        alert('Error: ' + resp.data);
      })['finally'](function () {
        $scope.verifying = false;
      })
    }
  })
}]);