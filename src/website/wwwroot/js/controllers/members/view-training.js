  angular.module('sarDatabase').controller('MemberTrainingCtrl', ['$scope', '$uibModal', 'MembersService',
  function ($scope, $uibModal, MembersService) {
    $.extend($scope, {
      latest: [],
      memberId: '',
      moreTraining: false,
      init: function(memberId) {
        $scope.memberId = memberId;
        MembersService.latestTraining($scope.latest, memberId)
        .then(null, function (reason) { console.log(reason); alert('error'); });
      },
      showMoreTraining: function () { $scope.moreTraining = true; }
    });
  }]);