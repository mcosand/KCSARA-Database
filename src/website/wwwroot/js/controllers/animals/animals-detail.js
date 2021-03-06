﻿angular.module('sar-database').controller("AnimalsDetailCtrl", ['$stateParams', '$state', '$scope', '$window', 'editorsService', 'animalsService',
  function ($stateParams, $state, $scope, $window, Editors, Animals) {
  angular.extend($scope, {
    tabs: [{state: 'ad_owners', name:'Owner(s)'}, {state: 'ad_missions', name:'Missions'}],
    animal: Animals.animals.one($stateParams.id).get().$object,
    selectedTab: 0,
    editAnimal: function (ev, animal) {
      Editors
        .doEditDialog(ev, '/wwwroot/partials/animals/edit.html', 'animal', animal)
        .then(function () { $scope.animal = Animals.animals.one($stateParams.id).get().$object });
    },
    deleteAnimal: function (ev, animal) {
      Editors
        .doDelete(ev, 'animal', animal)
        .then(function () { $state.go('animals'); });
    },
    pickPhoto: function (ev, animal) {
      $window.location.href = '/animals/photoupload/' + animal.id;
    }
  });

  $scope.$watch('selectedTab', function (current, old) {
    $state.go('animals_detail.' + $scope.tabs[current].state, { id: $stateParams.id });
  });
}]);