angular.module('sar-database')

  .provider('currentUser', function CurrentUserProvider() {
    this.$get = function CurrentUserFactory() {
      return {
        user: null
      };
    }
  })

.provider('authService', function AuthServiceProvider() {
  var options = {};

  this.useOptions = function(newOptions) {
    angular.extend(options, newOptions);
  };

  this.$get = ['currentUser', '$rootScope', '$window', '$http', function AuthServiceFactory(currentUser, $rootScope, $window, $http) {
    Oidc.Log.logger = console;
    Oidc.Log.level = Oidc.Log.INFO;
    
    var mgr = new Oidc.UserManager(options);

    var notifyChange = function () {
      $rootScope.$emit('auth-service-update');
    }

    function getStorageKey() {
      return "oidc.user:" + options.authority + ':' + options.client_id;
    }

    var service = {
      getUser: function AuthService_getUser() {
        return mgr.getUser();
      },
      bootstrapAuth: function (id_token, access_token, expires_at) {
        if (id_token && access_token && expires_at) {
          var packet = { id_token: id_token, access_token: access_token, expires_at: expires_at, isOpenIdConnect: true }
          currentUser.user = packet;
          mgr.settings.validator._processClaims(packet).then(function (user) {
            $window.sessionStorage.setItem(getStorageKey(), JSON.stringify(user));
            mgr._events._userLoaded.raise(user);
          })
        }
      },
      ensureAuthenticated: function () {
        alert('Not yet implemented');
      },
      signout: function () {
        console.log('begin signout');
        mgr.removeUser().then(function () {
          $window.location = '/logout';
          //mgr.signoutRedirect().then(function () {
          //  console.log('am signing out');
          //})
        })
      },
      signin: function () {
        $window.location = '/login';
      },
      subscribe: function (scope, callback) {
        var handler = $rootScope.$on('auth-service-update', callback);
        scope.$on('$destroy', handler);
      }
    };

    mgr.events.addUserLoaded(function (user) {
      currentUser.user = user;
      notifyChange();
    });

    return service;
  }];
});