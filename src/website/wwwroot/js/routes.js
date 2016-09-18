var routes = {
  'home': { url: '/', templateUrl: '/embedded/partials/home.html', resolve: { $title: function () { return 'Home'; } } },
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