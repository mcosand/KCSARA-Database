angular.module('sar-database')
.directive('eventList', [function () {
  return {
    restrict: 'E',
    transclude: true,
    scope: {
      list: '=',
      eventType: '@',
      linkTemplate: '@',
      actions: '='
    },
    templateUrl: '/wwwroot/partials/event-list.html'
  };
}])