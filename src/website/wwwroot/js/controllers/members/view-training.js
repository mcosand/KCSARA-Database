  angular.module('sarDatabase').controller('MemberTrainingCtrl', ['$scope', '$uibModal', 'MembersService',
  function ($scope, $uibModal, MembersService) {
    $.extend($scope, {
      latest: [],
      required: [],
      memberId: '',
      moreTraining: false,
      init: function(memberId) {
        $scope.memberId = memberId;
        MembersService.latestTraining($scope.latest, memberId)
        .then(null, function (reason) { console.log(reason); alert('error'); });
        MembersService.requiredTraining($scope.required, memberId)
        .then(null, function (reason) { console.log(reason); alert('error'); });
      },
      showMoreTraining: function () { $scope.moreTraining = true; },
      requiredDetails: function (model) {
        var modalInstance = $uibModal.open({
          templateUrl: window.appRoot + 'partials/members/required-detail.html',
          controller: 'MemberRequiredDetailCtrl',
          controllerAs: 'modal',
          size: 'md',
          resolve: {
            model: function () { return model }
          }
        });
        modalInstance.result
        .then(function (data) {
          //EventsService.logs($scope.logs, $scope.eventRoute, $scope.eventId);
        }, function (reason) {
          // don't have to do anything.
        });
      }
    });
  }]);

  angular.module('sarDatabase').controller('MemberRequiredDetailCtrl', ['$uibModalInstance', 'model', function ($uibModalInstance, model) {
    var modal = this;
    $.extend(modal, {
      model: model,
      dismiss: function (reason) {
        $uibModalInstance.dismiss(reason);
      }
    });
  }]);