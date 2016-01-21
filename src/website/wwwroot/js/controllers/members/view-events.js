  angular.module('sarDatabase').controller('MemberEventsCtrl', ['$scope', '$uibModal', 'MembersService',
  function ($scope, $uibModal, MembersService) {
    $.extend($scope, {
      events: [],
      memberId: '',
      eventType: '',
      showUnitColumn: true,
      eventTypeText: 'Event',
      init: function (memberId, eventType, showUnits, eventTypeText) {
        $scope.showUnitColumn = showUnits;
        $scope.memberId = memberId;
        $scope.eventType = eventType;
        $scope.eventTypeText = eventTypeText;
        MembersService.events($scope.events, memberId, eventType)
        .then(null, function (reason) { console.log(reason); alert('error'); });
      },
      showInfo: function (e) {
        $scope.detailedEvent = ($scope.detailedEvent == e) ? null : e;

        if ($scope.detailedEvent && !$scope.detailedEvent.timeline) {
          $scope.detailedEvent.timeline = [];
          MembersService.eventTimeline($scope.detailedEvent.timeline, $scope.memberId, $scope.eventType, e.id);
        }
      },
      tabLink: function (action, id) {
        return window.appRoot + $scope.eventRoute + '/' + id + '#/' + action;
      }
    });
  }]);