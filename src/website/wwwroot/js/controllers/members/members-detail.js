angular.module('sar-database').controller("MembersDetailCtrl", ['$stateParams', '$state', '$scope', '$window', 'editorsService', 'membersService',
  function ($stateParams, $state, $scope, $window, Editors, Members) {
  angular.extend($scope, {
    tabs: [{ state: 'md_missions', name: 'Missions' }, { state: 'md_training', name: 'Training' }],
    member: Members.members.one($stateParams.id).get().$object,
    currentPage: "md_missions",
    selectedTab: 0,
    editMember: function (ev, member) {
      alert('NYI')
      //Editors
      //  .doEditDialog(ev, '/wwwroot/partials/animals/edit.html', 'animal', animal)
      //  .then(function () { $scope.animal = Animals.animals.one($stateParams.id).get().$object });
    },
    deleteMember: function (ev, member) {
      Editors
        .doDelete(ev, 'member', member)
        .then(function () { $state.go('members'); });
    },
    pickPhoto: function (ev, member) {
      $window.location.href = '/members/photoupload/' + member.id;
    }
  });

  $scope.$watch('selectedTab', function (current, old) {
    $state.go('members_detail.' + $scope.tabs[current].state, { id: $stateParams.id });
  });
}]);