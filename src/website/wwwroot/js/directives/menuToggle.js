angular.module('sar-database')
.directive('menuToggle', ['$timeout', '$mdUtil', function ($timeout, $mdUtil) {
  return {
    scope: {
      section: '='
    },
    template:

      "<md-button class=\"md-button-toggle\"" +
  "ng-click=\"toggle()\"" +
  "aria-controls=\"docs-menu-{{section.name | nospace}}\"" +
  "aria-expanded=\"{{isOpen()}}\">" +
  "<div flex layout=\"row\">" +
  "<md-icon class=\"menu-icon\">{{section.icon}}</md-icon>" +
  "{{section.name}}" +
  "<span flex></span>" +
  "<span aria-hidden=\"true\" class=\"md-toggle-icon\"" +
  "ng-class=\"{'toggled' : isOpen()}\">" +
  "<md-icon class=\"menu-toggle\">keyboard_arrow_up</md-icon>" +
  "</span>" +
  "</div>" +
  "<span class=\"md-visually-hidden\">" +
  "Toggle {{isOpen()? 'expanded' : 'collapsed'}}" +
  "</span>" +
  "</md-button>" +

  "<ul id=\"docs-menu-{{section.name | nospace}}\" class=\"menu-toggle-list\">" +
  "<li ng-repeat=\"page in section.pages\">" +
    "<menu-link section=\"page\"></menu-link>" +
  "</li>" +
"</ul>",
    link: function ($scope, $element) {
      var controller = $element.parent().controller();

      $scope.isOpen = function () {
        return controller.isOpen($scope.section);
      };
      $scope.toggle = function () {
        controller.toggleOpen($scope.section);
      };

      $mdUtil.nextTick(function () {
        $scope.$watch(
          function () {
            return controller.isOpen($scope.section);
          },
          function (open) {
            // We must run this in a next tick so that the getTargetHeight function is correct
            $mdUtil.nextTick(function () {
              var $ul = $element.find('ul');
              var $li = $ul[0].querySelector('a.active');
              var docsMenuContent = $ul;
              while (docsMenuContent[0].tagName != "MD-CONTENT" && docsMenuContent[0].tagName != "BODY") docsMenuContent = docsMenuContent.parent();
              var targetHeight = open ? getTargetHeight() : 0;

              $timeout(function () {
                // Set the height of the list
                $ul.css({ height: targetHeight + 'px' });

                // If we are open and the user has not scrolled the content div; scroll the active
                // list item into view.
                if (open && $li && $li.offsetParent && $ul[0].scrollTop === 0) {
                  $timeout(function () {
                    var activeHeight = $li.scrollHeight;
                    var activeOffset = $li.offsetTop;
                    var parentOffset = $li.offsetParent.offsetTop;

                    // Reduce it a bit (2 list items' height worth) so it doesn't touch the nav
                    var negativeOffset = activeHeight * 2;
                    var newScrollTop = activeOffset + parentOffset - negativeOffset;

                    $mdUtil.animateScrollTo(docsMenuContent, newScrollTop);
                  }, 350, false);
                }
              }, 0, false);

              function getTargetHeight() {
                var targetHeight;
                $ul.addClass('no-transition');
                $ul.css('height', '');
                targetHeight = $ul.prop('clientHeight');
                $ul.css('height', 0);
                $ul.removeClass('no-transition');
                return targetHeight;
              }
            }, false);
          }
        );
      });

      var parentNode = $element[0].parentNode.parentNode.parentNode;
      if (parentNode.classList.contains('parent-list-item')) {
        var heading = parentNode.querySelector('h2');
        $element[0].firstChild.setAttribute('aria-describedby', heading.id);
      }
    }
  };
}])
