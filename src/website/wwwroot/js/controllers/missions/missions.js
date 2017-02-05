angular.module('sar-database').controller("MissionsCtrl", ['$stateParams', '$scope', 'listService', 'missionsService',
  function ($stateParams, $scope, Lists, Missions) {
    angular.extend($scope, {
      missions: Lists.eventsLoader(Missions.missions, { miles: true, persons: true })
    })
  }]);