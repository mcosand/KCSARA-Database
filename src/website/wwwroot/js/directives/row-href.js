angular.module('sar-database')
.directive('rowHref', ['$compile', function ($compile) {
  return {
    restrict: 'A',
    scope: true,
    link: function (scope, element, attributes) {
      var cells = element.children("td[skip-href!='yes']")
      for (var i = 0; i < cells.length; i++) {
        var cell = angular.element(cells[i])
        var link = angular.element('<a ng-href="' + attributes.rowHref + '"' + (attributes.rowHrefExt !== undefined ? ' target="_self"' : '') + '>' + cell.html() + '</a>')
        $compile(link)(scope.$parent);
        cell.empty()
        cell.append(link)
      }
    }
  };
}]);