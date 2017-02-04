angular.module('sar-database').controller("AnimalsMissionsCtrl", ['$stateParams', '$window', '$scope', '$rootScope', 'listService', 'animalsService',
  function ($stateParams, $window, $scope, $rootScope, Lists, Animals) {
    angular.extend($scope, {
      stats: Animals.animals.one($stateParams.id).all('missions').one('stats').get().$object,
      missions: Lists.eventsLoader(Animals.animals.one($stateParams.id).all('missions')),
      gotoMission: function (mission) {
        $window.location.href = '/missions/roster/' + mission.event.id;
      }
    })
    
  }]);