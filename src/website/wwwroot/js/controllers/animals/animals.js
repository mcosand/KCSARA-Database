angular.module('sar-database').controller("AnimalsCtrl", ['$stateParams', '$state', '$scope', 'editorsService', 'animalsService', function ($stateParams,$state, $scope, Editors, Animals) {
  refreshAnimals = function Animals_Refresh() {
    $scope.loading = Animals.animals.getList().then(function (list) {
      $scope.animals = list
      $scope.types = []
      var dict = {};
      for (var i = 0; i < list.length; i++) {
        if (!dict[list[i].type]) $scope.types.push(list[i].type)
        dict[list[i].type] = true;
      }
      $scope.types.sort();
    });
  }

  angular.extend($scope, {
    animals: [],
    types: [],
    activeOnly: true,
    showTypes: 'any',
    doFilter: function (item) {
      return (item.status == 'active' || !$scope.activeOnly) && ($scope.showTypes == 'any' || $scope.showTypes == item.type);
    },
    gotoAnimal: function (animal) {
      $state.go('animals_detail', { id: animal.id })
    },
    createNew: function (ev) {
      var animal = Animals.create();
      Editors
        .doEditDialog(ev, '/wwwroot/partials/animals/create.html', 'animal', animal, { types: $scope.types })
        .then(function (newAccount) {
          $scope.animals.push(newAccount);
        });
    }
  })

  refreshAnimals();
}]);