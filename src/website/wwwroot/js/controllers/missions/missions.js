angular.module('sar-database').controller("MissionsCtrl", ['$stateParams', '$scope', 'listService', 'missionsService', 'editorsService',
  function ($stateParams, $scope, Lists, Missions, Editors) {
    angular.extend(this, {
      missions: Lists.pagedEventsLoader(Missions.years, Missions.missions, { miles: true, persons: true, showDelete: true }),
      actions: {
        deleteEvent: function (ev, mission) {
          Editors
            .doDelete(ev, 'mission', mission, { getName: function (m) { return m.event.name } })
            .then(function () { $scope.missions.getPages(); });
        }
      }
    })
  }]);