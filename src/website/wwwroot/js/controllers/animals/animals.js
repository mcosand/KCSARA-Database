﻿angular.module('sar-database').controller("AnimalsCtrl", ['$stateParams', '$state', '$scope', 'editorsService', 'animalsService', function ($stateParams,$state, $scope, Editors, Animals) {
  refreshAnimals = function Animals_Refresh() {
    $scope.loading = Animals.animals.getList().then(function (list) {
      $scope.animals = list
    });
  }

  angular.extend($scope, {
    animals: [],
    activeOnly: true,
    showTypes: 'any',
    doFilter: function (item) {
      return (item.status == 'active' || !$scope.activeOnly) && ($scope.showTypes == 'any' || $scope.showTypes == item.type);
    },
    gotoAnimal: function (animal) {
      $state.go('animals_detail.ad_owners', { id: animal.id })
    },
    createNew: function (ev) {
      var animal = Animals.create();
      Editors
        .doEditDialog(ev, '/wwwroot/partials/animals/edit.html', 'animal', animal)
        .then(function (newAnimal) {
          var owner = Animals.createOwner(newAnimal);
          owner.isPrimary = true;
          return Animals.editOwnerDialog(ev, owner);
        })
        .then(function (newOwnership) {
          refreshAnimals();
        });
    }
  })

  refreshAnimals();
}]);