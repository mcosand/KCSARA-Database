angular.module('sar-database')
.directive('eventList', [function () {
  return {
    restrict: 'E',
    scope: {
      list: '=',
      eventType: '@',
      linkTemplate: '@'
    },
    templateUrl: '/wwwroot/partials/event-list.html'
//    link: function (scope, element) {
//    }
  };
}])