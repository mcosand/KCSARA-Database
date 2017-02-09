angular.module('sar-database').controller("MembersContactsCtrl", ['$stateParams', '$window', '$scope', '$rootScope', 'listService', 'membersService',
  function ($stateParams, $window, $scope, $rootScope, Lists, Members) {
    var restMember = Members.members.one($stateParams.id)
    angular.extend($scope, {
      contacts: Lists.loader(restMember.all('contacts'), { load: true }),
      addresses: Lists.loader(restMember.all('addresses'), { load: true })
    })
  }]);
