angular.module('sar-database')
.directive('requiredTraining', ['$state', '$filter', function ($state, $filter) {
  var simpleDate = $filter('simpleDate')

  function update(element, scope) {
    element.removeClass('exp_Missing').removeClass('exp_Expired').removeClass('exp_NotExpired')
    if (scope.$parent.required.list.loading) { element.text(''); return }
    var status = scope.requiredTraining
    if (!status) {
      element.text('Unknown');
    } else if (!status.completed) {
      element.text('Missing').addClass('exp_Missing');
    } else if (!status.expires) {
      element.text('Complete')
    } else {
      var expiry = simpleDate(status.expires)
      var today = simpleDate(new Date())
      element.text(simpleDate(status.expires)).addClass(expiry < today ? 'exp_Expired' : 'exp_NotExpired')
    }
  }
  
  return {
    restrict: 'A',
    scope: {
      requiredTraining: '='
    },
    link: function (scope, element) {
      update(element, scope)
      scope.$watch('requiredTraining', function () {
        console.log(scope.requiredTraining)
        update(element, scope)
      })
      //var cells = element.children("td[skip-href!='yes']")
      //for (var i = 0; i < cells.length; i++) {
      //  var cell = angular.element(cells[i])
      //  var link = angular.element('<a ng-href="' + attributes.rowHref + '"' + (attributes.rowHrefExt !== undefined ? ' target="_self"' : '') + '>' + cell.html() + '</a>')
      //  $compile(link)(scope.$parent);
      //  cell.empty()
      //  cell.append(link)
      //}
    }
  };
}]);