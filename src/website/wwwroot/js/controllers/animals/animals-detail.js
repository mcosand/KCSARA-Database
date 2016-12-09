angular.module('sar-database').controller("AnimalsDetailCtrl", ['$stateParams', '$state', '$scope', 'editorsService', 'animalsService', function ($stateParams, $state, $scope, Editors, Animals) {
  angular.extend($scope, {
    animal: Animals.animals.one($stateParams.id).get().$object,
    currentPage: "ad_owners"
  });
}]);