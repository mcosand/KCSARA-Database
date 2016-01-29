angular.module('sarDatabase').controller('MembersDetailCtrl', ['$scope', '$location',
  function ($scope, $location) {
    function parseTabFromhash(value) {
      return (value.match(/^#\/?([^\/]+)/) || ['#/info', 'info'])[1];
    };

    $.extend($scope, {
      activeTab: parseTabFromhash($location.path()),
      init: function (memberId) {
        $scope.memberId = memberId;
      },
      setTab: function (newTab) {
        if ($location.path() == '') { $location.replace(); }
        $location.path(newTab);
      }
    });

    $scope.$watch(function () { return location.hash }, function (value) { $scope.activeTab = parseTabFromhash(value); });
  }]);