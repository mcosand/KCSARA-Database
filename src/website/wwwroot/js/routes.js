{
  var resolveUnitTitle =
  {
    unit: ['unitsService', '$stateParams', function (Units, $stateParams) {
      // Use Contacts service to retrieve a contact
      return Units.units.get({ id: $stateParams.id });
    }],
    // Dynamic title showing the name of contact
    $title: ['unit', function (unit) {
      return unit.name;
    }]
  };

  var routes = {
    'home': { url: '/', templateUrl: '/embedded/partials/home.html', resolve: { $title: function () { return 'Home'; } } },
    'units': { url: '/units', templateUrl: '/wwwroot/partials/units/list.html', resolve: { $title: function () { return 'Units' } } },
    'unitsDetail': { url: '/units/detail/:id', templateUrl: '/wwwroot/partials/units/detail.html', resolve: resolveUnitTitle },
    'unitsRoster': { url: '/units/roster/:id', templateUrl: '/wwwroot/partials/units/roster.html', resolve: resolveUnitTitle },
    'accounts': { url: '/accounts', template: '<div>Accounts page</div>', resolve: { $title: function () { return 'Accounts'; } } },
    'accounts_me': { url: '/accounts/me', templateUrl: '/wwwroot/partials/accounts/me.html', resolve: { $title: function () { return 'My Account'; } } }
  };

  angular.module('sar-database')
  .config(['$locationProvider', '$stateProvider', '$urlRouterProvider', function ($locationProvider, $stateProvider, $urlRouterProvider) {
    console.log('running');
    $locationProvider.html5Mode(true)
    for (var r in routes) {
      $stateProvider.state(r, routes[r]);
    }
    $urlRouterProvider.otherwise('/');
  }]);
};