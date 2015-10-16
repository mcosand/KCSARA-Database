(function (l) {
  l.module("treeView", []).directive("treeModel", ['$compile', function ($compile) {
    return {
      restrict: "A",
      link: function (scope, g, c) {
        var e = c.treeModel,
          h = c.nodeLabel || "label",
          d = c.nodeChildren || "children",
          canEdit = scope[c.canEdit] || false,
        k = '<ul class="no-bullet">' +
              '<li data-ng-repeat="node in ' + e + '">' +
                ' <div data-ng-click="selectNodeHead(node, $event)">' +
                  '<input type="checkbox" class="input-control" ng-model="node.remove" ng-show="' + (canEdit && !(c.treeNested || false)) + '" ng-click="clickInput(node, $event)">' +
                  '<i class="fa fa-fw fa-plus-square-o" data-ng-show="!node.' + d + '.loaded || (node.' + d + '.length && node.collapsed)" data-ng-click="selectNodeHead(node, $event)"></i>' +
                  '<i class="fa fa-fw fa-minus-square-o" data-ng-show="node.' + d + '.length && !node.collapsed" data-ng-click="selectNodeHead(node, $event)"></i>' +
                  '<i class="fa fa-fw" data-ng-hide="!node.' + d + '.loaded || node.' + d + '.length"></i>' +
                  ' {{node.' + h + '}}' +
                  '</div>' +
                '<div data-ng-hide="!node.' + d + '.loaded || node.collapsed" data-tree-model="node.' + d + '" data-node-label="' + h + '" data-node-children="' + d + '" data-tree-nested="true"></div>' +
              '</li>' +
            '</ul>';
        e && e.length && (c.angularTreeview ? (
        scope.$watch(
          e,
          function (m, b) { g.empty().html($compile(k)(scope)) },
          !1),
        scope.selectNodeHead = scope.selectNodeHead || function (ax, b) {
          b.stopPropagation && b.stopPropagation();
          b.preventDefault && b.preventDefault();
          b.cancelBubble = !0;
          b.returnValue = !1;
          ax.collapsed = ax.collapsed === undefined ? false : !ax.collapsed;
          if (c.onSelected) {
            scope[c.onSelected](ax);
          }
        },
        scope.clickInput = scope.removeFromGroup || function (node, ev) {
          ev.stopPropagation && ev.stopPropagation();
          ev.cancelBubble = !0;
        }
          )
        : g.html($compile(k)(scope)))
      }
    }
  }])
})(angular);
