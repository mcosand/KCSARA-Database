angular.module('sarDatabase').controller('LoginModalCtrl', ['$uibModalInstance', '$q', '$http',
  function ($uibModalInstance, $q, $http) {
    var modal = this;
    $.extend(modal, {
      username: '',
      password: '',
      dismiss: function (reason) {
        $uibModalInstance.dismiss(reason);
      },
      submit: function () {
        $http({
          method: 'POST',
          url: window.appRoot + 'api/account/login',
          data: { username: modal.username, password: modal.password }
        })
        .success(function (data) {
          $uibModalInstance.close(modal.model);
        })
      }
    });
  }]);