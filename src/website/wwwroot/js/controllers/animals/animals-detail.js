angular.module('sar-database').controller("AnimalsDetailCtrl", ['$stateParams', '$state', '$scope', 'editorsService', 'animalsService', function ($stateParams, $state, $scope, Editors, Animals) {
  angular.extend($scope, {
    tabs: [{state: 'ad_owners', name:'Owner(s)'}, {state: 'ad_missions', name:'Missions'}],
    animal: Animals.animals.one($stateParams.id).get().$object,
    currentPage: "ad_owners",
    selectedTab: 0
  });

  $scope.$watch('selectedTab', function (current, old) {
    $state.go('animals_detail.' + $scope.tabs[current].state, { id: $stateParams.id });
  });
}]);