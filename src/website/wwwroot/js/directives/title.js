angular.module('sar-database').directive('title', ['$rootScope', '$timeout',
  function ($rootScope, $timeout) {
    return {
      link: function () {

        var listener = function (event, toState) {

          $timeout(function () {
            $rootScope.pageTitle = (toState.data && toState.data.pageTitle)
            ? toState.data.pageTitle
            : 'Database';
          });
        };

        $rootScope.$on('$stateChangeSuccess', listener);
      }
    };
  }
]);