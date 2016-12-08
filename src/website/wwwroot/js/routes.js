﻿{
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
      'home': { url: '/', templateUrl: '/embedded/partials/home.html', data: { allowAnonymous: true }, resolve: { $title: function () { return 'Home'; } } }
    , 'units': { url: '/units', templateUrl: '/wwwroot/partials/units/list.html', resolve: { $title: function () { return 'Units' } } }
    , 'unitsDetail': { url: '/units/detail/:id', templateUrl: '/wwwroot/partials/units/detail.html', resolve: resolveUnitTitle }
    , 'unitsRoster': { url: '/units/roster/:id', templateUrl: '/wwwroot/partials/units/roster.html', resolve: resolveUnitTitle }
    , 'training': { url: '/training', templateUrl: '/wwwroot/partials/training/index.html', resolve: { $title: function() { return 'Training' }}}
    , 'training_courselist': { url: '/training/courses', templateUrl: '/wwwroot/partials/training/course-list.html', resolve: { $title: function () { return 'Course List' } } }
    , 'training_coursedetail': { url: '/training/courses/:id', templateUrl: '/wwwroot/partials/training/course-detail.html', resolve: { $title: function () { return 'Course Detail' } } }
    , 'training_courseroster': { url: '/training/courses/:id/roster', templateUrl: '/wwwroot/partials/training/course-roster.html', resolve: { $title: function () { return 'Course Detail' } } }
    , 'trainingBatchUpload': { url: '/training/uploadrecords', templateUrl: '/wwwroot/partials/training/upload-records.html', resolve: { $title: function () { return 'Upload Training Records' } } }
    , 'accounts': { url: '/accounts', templateUrl: '/wwwroot/partials/accounts/list.html', resolve: { $title: function () { return 'Accounts'; } } }
    , 'account_reset': { url: '/accounts/reset', templateUrl: '/wwwroot/partials/accounts/reset.html', data: { allowAnonymous: true }, resolve: { $title: function () { return 'Account Reset' } } }
    , 'account_reset_finish': { url: '/accounts/reset/:code', templateUrl: '/wwwroot/partials/accounts/reset-finish.html', data: { allowAnonymous: true }, resolve: { $title: function () { return 'Account Reset' } } }
    , 'accounts_me': { url: '/accounts/me', templateUrl: '/wwwroot/partials/accounts/detail.html', resolve: { $title: function () { return 'My Account'; } } }
    , 'accounts_detail': { url: '/accounts/:id', templateUrl: '/wwwroot/partials/accounts/detail.html', resolve: { $title: function () { return 'Account Detail'; } } }
    , 'animals': { url: '/animals', templateUrl: '/wwwroot/partials/animals/list.html', resolve: { $title: function () { return "Animals"; } } }
    , 'animals_detail': { url: '/animals/:id', template: 'Here we are', resolve: { $title: function() { return 'Animal Detail' }}}
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
  .config(['$locationProvider', '$stateProvider', '$urlRouterProvider', '$urlMatcherFactoryProvider',
    function ($locationProvider, $stateProvider, $urlRouterProvider, $urlMatcherFactoryProvider) {
      $urlMatcherFactoryProvider.strictMode(false);
      $urlMatcherFactoryProvider.caseInsensitive(true);
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