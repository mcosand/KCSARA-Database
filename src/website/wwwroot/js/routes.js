var routes = {
  'home': {url: '/', templateUrl:'/embedded/partials/home.html' },
  'accounts':{ url: '/accounts', template: '<div>Accounts page</div>' },
  'accounts_me': { url: '/accounts/me', templateUrl: '/assets/partials/accounts/me.html' }
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