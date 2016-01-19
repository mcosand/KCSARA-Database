  angular.module('sarDatabase').controller('MemberContactsCtrl', ['$scope', '$uibModal', 'MembersService',
  function ($scope, $uibModal, MembersService) {
    $.extend($scope, {
      contacts: [],
      addresses: [],
      memberId: '',
      init: function(memberId) {
        $scope.memberId = memberId;
        MembersService.contacts($scope.contacts, memberId)
        .then(null, function (reason) { console.log(reason); alert('error'); });
        MembersService.addresses($scope.addresses, memberId)
        .then(null, function (reason) { console.log(reason); alert('error'); });
      },
      startAddContact: function () {
        alert('add contact');
      },
      startAddAddress: function () {
        alert('add address');
      }
    });
  }]);