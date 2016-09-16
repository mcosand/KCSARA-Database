﻿angular.module('sar-database')
.directive('menuLink', function () {
  return {
    scope: {
      section: '='
    },
    template: "<md-button ng-class=\"{'active' : isSelected()}\" ng-href=\"{{section.url}}\" ng-click=\"focusSection()\"><md-icon class=\"menu-icon\">{{section.icon}}</md-icon> {{section | humanizeDoc}}"
      + "<span class=\"md-visually-hidden\" ng-if=\"isSelected()\">current page</span></md-button>",
    link: function ($scope, $element) {
      var controller = $element.parent().controller();

      $scope.isSelected = function () {
        return controller.isSelected($scope.section);
      };

      $scope.focusSection = function () {
        // set flag to be used later when
        // $locationChangeSuccess calls openPage()
        controller.autoFocusContent = true;
      };
    }
  };
});