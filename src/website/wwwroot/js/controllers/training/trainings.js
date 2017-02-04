angular.module('sar-database').controller("TrainingsCtrl", ['$stateParams', '$scope', 'listService', 'trainingService',
  function ($stateParams, $scope, Lists, Training) {
    angular.extend($scope, {
      trainings: Lists.eventsLoader(Training.trainings, { miles: true, persons: true })
    })
  }]);