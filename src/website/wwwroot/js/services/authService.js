angular.module('sar-database')

  .provider('currentUser', function CurrentUserProvider() {
    this.$get = ['$rootScope', function CurrentUserFactory($rootScope) {
      var instance = {
        user: null
      };
      $rootScope.$currentUser = instance;
      return instance;
    }]
  })

.provider('authService', function AuthServiceProvider() {
  var options = {};

  this.useOptions = function (newOptions) {
    angular.extend(options, newOptions);
  };

  this.$get = ['currentUser', '$rootScope', '$state', '$window', '$http', '$q', '$timeout', function AuthServiceFactory(currentUser, $rootScope, $state, $window, $http, $q, $timeout) {
    Oidc.Log.logger = console;
    Oidc.Log.level = Oidc.Log.INFO;

    var storageKey = "oidc.user:" + options.authority + ':' + options.client_id;
    var storedUser = JSON.parse($window.sessionStorage.getItem(storageKey) || 'null');
    if (storedUser) {
      if (storedUser.expires_at && storedUser.expires_at < Math.floor(Date.now() / 1000) - 20) {
//        console.log('%% stored user was expired. Dumped it.')
        $window.sessionStorage.removeItem(storageKey);
        storedUser = null;
      }
      else {
        currentUser.user = storedUser;
      }
    }

    var mgr = new Oidc.UserManager(options);

    var service;
    service = {
      getUser: function AuthService_getUser() {
        return mgr.getUser().then(function (user) {
          currentUser.user = user;
          return user;
        });
      },
      signout: function () {
        var token = currentUser.user.id_token;
        mgr.removeUser().then(function () {
          mgr.signoutRedirect({ id_token_hint: token });
        })
      },
      startLogin: function AuthService_startLogin() {
        $window.sessionStorage.setItem('oidc:returnState', JSON.stringify({ name: $state.next.name, p: $state.toParams }));
        mgr.signinRedirect();
      },
      finishLoginAsync: function AuthService_finishLogin() {
        console.log('%% finishing login')
        return mgr.signinRedirectCallback().then(function (user) {
          currentUser.user = user;
          var returnState = { name: 'home' };
          var storedTarget = $window.sessionStorage.getItem('oidc:returnState');
          if (storedTarget) {
            $window.sessionStorage.removeItem('oidc:returnState');
            returnState = JSON.parse(storedTarget);
          }
          return returnState;
        });
      },
      ensureAuthenticatedAsync: function () {
        return service.getUser().then(function (user) {

          currentUser.user = user;
          if (user == null) {
            currentUser.loggingIn = true;
            service.startLogin();
            return $q.reject;
          }
        });
      },
      subscribe: function (scope, callback) {
        var handler = $rootScope.$on('auth-service-update', callback);
        scope.$on('$destroy', handler);
      }
    };

    var notifyChange = function (msg) {
      $rootScope.$emit('auth-service-update', msg);
    }

    mgr.events.addUserLoaded(function (user) {
      currentUser.user = user;
      notifyChange({ action: 'loaded' });
    });
    mgr.events.addUserUnloaded(function () {
      currentUser.user = null;
    })
    mgr.events.addAccessTokenExpiring(function () {
      console.log('ALERT: token expiring');
      var remaining = Math.max(currentUser.user.expires_at - Math.ceil(Date.now() / 1000), 0);
      notifyChange({ action: 'expiring', remaining: remaining });
    })
    mgr.events.addAccessTokenExpired(function () {
      mgr.removeUser().then(function () {
        currentUser.user = null;
        notifyChange({ action: 'expired' });
      })
    });
    mgr.events.addUserSignedOut(function () {
      currentUser.user = null;
    });

    $rootScope.startLogin = service.startLogin;

    return service;
  }];
});