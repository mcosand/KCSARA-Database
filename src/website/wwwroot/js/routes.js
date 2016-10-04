{
  var resolveUnitTitle =
  {
    unit: ['unitsService', '$stateParams', function (Units, $stateParams) {
      return Units.units.one($stateParams.id).get();
    }],
    $title: ['unit', function (unit) {
      return unit.name;
    }]
  };

  var resolveUserLoaded = ['authService', function ResolveUserLoaded(Auth) {
    return Auth.getUser();
  }];
  var resolveUserSignedIn = ['authService', function ResolveUserSignedIn(Auth) {
    return Auth.ensureAuthenticatedAsync();
  }];

  var routes = {
    'home': { url: '/', templateUrl: '/embedded/partials/home.html', data: { allowAnonymous: true }, resolve: { $title: function () { return 'Home'; } } },
    'units': { url: '/units', templateUrl: '/wwwroot/partials/units/list.html', resolve: { $title: function () { return 'Units' } } },
    'unitsDetail': { url: '/units/detail/:id', templateUrl: '/wwwroot/partials/units/detail.html', resolve: resolveUnitTitle },
    'unitsRoster': { url: '/units/roster/:id', templateUrl: '/wwwroot/partials/units/roster.html', resolve: resolveUnitTitle },
    'accounts': { url: '/accounts', template: '<div>Accounts page</div>', resolve: { $title: function () { return 'Accounts'; } } },
    'accounts_me': { url: '/accounts/me', templateUrl: '/wwwroot/partials/accounts/me.html', resolve: { $title: function () { return 'My Account'; } } },
    'training': { url: '/training/uploadrecords', templateUrl: '/wwwroot/partials/training/upload-records.html', resolve: { $title: function () { return 'Upload Training Records' }}}
  };

  angular.module('sar-database')
  .config(function ($provide) {
    $provide.decorator('$state', function ($delegate, $rootScope) {
      $rootScope.$on('$stateChangeStart', function (event, state, params) {
        $delegate.next = state;
        $delegate.toParams = params;
      });
      return $delegate;
    });
  })
  .config(['$locationProvider', '$stateProvider', '$urlRouterProvider', function ($locationProvider, $stateProvider, $urlRouterProvider) {
    $locationProvider.html5Mode(true)
    for (var r in routes) {
      var opts = routes[r];

      opts.resolve = opts.resolve || {};
      opts.resolve._userLoaded = (opts.data && opts.data.allowAnonymous)
                                      ? resolveUserLoaded
                                      : resolveUserSignedIn;
      $stateProvider.state(r, opts);
    }
    $stateProvider.state('login_callback', {
      url: '/loggedIn', template: 'Logging in ...', data: { allowAnonymous: true }, resolve: {
        finishLogin: ['authService', '$timeout', '$state', function (Auth, $timeout, $state) {
          return Auth.finishLoginAsync().then(function (newState) {
            $timeout(function () { $state.go(newState.name, newState.p) });
          })
        }]
      }
    });
    $urlRouterProvider.otherwise('/');
  }]);
};