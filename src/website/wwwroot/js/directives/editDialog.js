angular.module('sar-database')
  .directive('editDialog', function () {
  return {
    restrict: 'E',
    replace: true,
    transclude: true,
    templateUrl: '/wwwroot/partials/editDialog.html'
  };
});