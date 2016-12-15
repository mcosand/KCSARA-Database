angular.module('sar-database').controller("AnimalsOwnersCtrl", ['$stateParams', '$state', '$scope', '$rootScope', '$http', 'editorsService', 'animalsService',
  function ($stateParams, $state, $scope, $rootScope, $http, Editors, Animals) {
  angular.extend($scope, {    
    owners: {
      query: {
        order: '-isPrimary'
      },
      list: [],
      getList: function () {
        $scope.owners.loading = Animals.animals.one($stateParams.id).all('owners').getList().then(function (data) {
          if (data.length > 0) data[0].isPrimary = true;
          $scope.owners.list = data;
        })
      },
    },
    createOwner: function(ev) {
      var owner = Animals.createOwner($scope.animal);
      $scope.editOwner(ev, owner);
    },
    editOwner: function(ev, owner)
    {
      Animals.editOwnerDialog(ev, owner)
      .then(function () { $scope.owners.getList(); });
    },
    deleteOwner: function (ev, owner) {
      owner.name = owner.member.name
      Editors
        .doDelete(ev, 'owner', owner)
        .then(function () { $scope.owners.getList(); });
    }
  })

  $scope.owners.getList();
}]);