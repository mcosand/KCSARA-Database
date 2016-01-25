  angular.module('sarDatabase').controller('MemberInfoCtrl', ['$scope', '$uibModal', 'MembersService',
  function ($scope, $uibModal, MembersService) {
    $.extend($scope, {
      memberships: [],
      memberId: '',
      isSelf: true,
      viewMedical: false,
      init: function(memberId, isSelf) {
        $scope.memberId = memberId;
        $scope.isSelf = isSelf;
        MembersService.memberships($scope.memberships, memberId)
        .then(null, function (reason) { console.log(reason); alert('error'); });
      },
      toggleMedical: function(evt) {
        if (!$scope.isSelf) {
          evt.preventDefault();
          evt.stopPropagation();
          alert("This information is private.");
          return false;
        }
        $scope.viewMedical = !$scope.viewMedical;
        if ($scope.viewMedical && !$scope.medical) {
          $scope.medical = {};
          MembersService.medical($scope.medical, $scope.memberId);
        }
      },
      startAddMembership: function () {
        alert('add membership');
      }
    });
  }]);