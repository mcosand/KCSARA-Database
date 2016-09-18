angular.module('sar-database')
.directive('menuLink', ['$state', function ($state) {
  return {
    scope: {
      section: '='
    },
    template: "<md-button ng-class=\"{'active' : isSelected()}\" ng-attr-target=\"{{isExternal() ? '_self' : undefined}}\" ng-href=\"{{section.url}}\"><md-icon class=\"menu-icon\">{{section.icon}}</md-icon> {{section | humanizeDoc}}"
      + "<span class=\"md-visually-hidden\" ng-if=\"isSelected()\">current page</span></md-button>",
    link: function ($scope, $element) {
      var controller = $element.parent().controller();

      $scope.isSelected = function () {
        return controller.isSelected($scope.section);
      };

      $scope.isExternal = function () {
        var states = $state.get();
        var match = states.filter(function (item) {
          return item.url == $scope.section.url;
        });
        return match.length == 0;
      }

      $scope.focusSection = function () {
        // set flag to be used later when
        // $locationChangeSuccess calls openPage()
        controller.autoFocusContent = true;
      };
    }
  };
}]);