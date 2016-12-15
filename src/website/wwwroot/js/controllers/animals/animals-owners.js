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
    editOwner: function (ev, owner) {
      owner.wasPrimary = owner.isPrimary
      Editors
        .doEditDialog(ev, '/wwwroot/partials/animals/edit-owner.html', 'owner', owner, {
          nameResolver: function(item) { return item.member.name },
          getPhotoUrl: function (member) {
            var user = $rootScope.$currentUser.user;
            if (user && user.access_token)
              return '/api2/members/' + member.id + '/photo?access_token=' + user.access_token
            return '/content/images/nophoto.jpg'
          },
          search: {
            menuSearchOpen: false,
//            showSearchMenu: function () { $mdSidenav('searchMenu').open(); },
            querySearch: function (text) {
              if (!text) return [];

              return $http({
                method: 'GET',
                url: window.appRoot + 'api2/search?t=Member&q=' + encodeURIComponent(text)
              }).then(function successCallback(response) {
                return response.data.map(function(item) { return item.summary });
              }, function errorCallback(response) {
                // called asynchronously if an error occurs
                // or server returns response with an error status.
              });
            },
            selectedItemChange: function (item) {
              console.log('picked!!', item)
            }
          }
        })
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