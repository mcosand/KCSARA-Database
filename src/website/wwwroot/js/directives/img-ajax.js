angular.module('sar-database')
  .directive('ajax', ['$http', function ($http) {
    function update(element, scope) {
      console.log('UPDATING img ', scope.ajax)
      element.css('background-image', '')
      $http({
        method: 'GET',
        url: scope.ajax + ((scope.noCache == 'true') ? '?ts=' + new Date().getTime() : '')
      }).then(function successCallback(response) {
        if (scope.backgroundImage) {
          element.css('background-image', "url('" + response.data.data + "')")
        } else {
          element.attr('src', response.data.data)
        }
      })
    }

  return {
    restrict: 'A',
    scope: {
      ajax: '@',
      backgroundImage: '@',
      noCache: '@'
    },
    link: function (scope, element) {
      update(element, scope)
      scope.$watch('ajax', function () {
        console.log(scope)
        update(element, scope)
      })
    }
  };
}]);