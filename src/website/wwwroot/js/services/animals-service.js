angular.module('sar-database')

.provider('animalsService', function AnimalsServiceProvider() {
  this.$get = ['$rootScope', '$http', 'Restangular', 'editorsService', 'authService', function AnimalServiceFactory($rootScope, $http, Restangular, Editors) {
    var result = {
      animals: Restangular.service('animals'),
      create: function () { return Restangular.restangularizeElement(null, {}, '/animals') },
      createOwner: function (animal) { return Restangular.restangularizeElement(null, { animal: animal }, '/animals/' + animal.id + '/owners') },

      editOwnerDialog: function (ev, owner) {
        owner.wasPrimary = owner.isPrimary
        return Editors
          .doEditDialog(ev, '/wwwroot/partials/animals/edit-owner.html', 'owner', owner, {
            nameResolver: function (item) { return item.member.name },
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
                  url: 'https://kcsara-api2.azurewebsites.net/search?t=Member&q=' + encodeURIComponent(text)
                }).then(function successCallback(response) {
                  return response.data.map(function (item) { return item.summary });
                }, function errorCallback(response) {
                  // called asynchronously if an error occurs
                  // or server returns response with an error status.
                });
              }
            }
          })
      },
    };

    return result;
  }]
});