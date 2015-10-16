angular.module('sarDatabase').directive('statusGlyph', function () {
  return {
    restrict: 'E',
    template: "<i ng-class=\"{'text-success': test, 'fa-check': test, 'fa-exclamation': !test, 'text-danger': !test}\" class=\"fa\"></i>",
    scope: {
      test: '='
    }
  }
});