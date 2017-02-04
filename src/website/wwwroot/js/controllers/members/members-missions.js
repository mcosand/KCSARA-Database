angular.module('sar-database').controller("MembersMissionsCtrl", ['$stateParams', '$scope', 'listService', 'membersService',
  function ($stateParams, $scope, Lists, Members) {
    angular.extend($scope, {
      stats: Members.members.one($stateParams.id).all('missions').one('stats').get().$object,
      missions: Lists.eventsLoader(Members.members.one($stateParams.id).all('missions'), { miles: true })
    })
  }]);