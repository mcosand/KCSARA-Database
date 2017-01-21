angular.module('sar-database').controller("MembersMissionsCtrl", ['$stateParams', '$window', '$scope', '$rootScope', 'membersService',
  function ($stateParams, $window, $scope, $rootScope, Members) {
    angular.extend($scope, {
      stats: Members.members.one($stateParams.id).all('missions').one('stats').get().$object,
      missions: {
        query: {
          order: '-event.startTime'
        },
        list: [],
        years: [],
        yearlyStats: {},
        showYear: null,
        getList: function () {
          $scope.missions.loading = Members.members.one($stateParams.id).all('missions').getList().then(function (data) {
            $scope.missions.list = data;
            $scope.missions.years = [];
            $scope.missions.yearlyStats = {};
            var year = null;
            for (var i = 0; i < data.length; i++) {
              var y = data[i].event.start.substring(0, 4);
              if (y != year) $scope.missions.years.push(y);
              if (!$scope.missions.showYear) $scope.missions.showYear = y;
              year = y;
              $scope.missions.yearlyStats[y] = $scope.missions.yearlyStats[y] || { count: 0, hours: 0, miles: 0 };
              $scope.missions.yearlyStats[y].count++;
              $scope.missions.yearlyStats[y].hours += data[i].hours;
              $scope.missions.yearlyStats[y].miles += data[i].miles;
              $scope.missions.yearlyStats['all'] = $scope.missions.yearlyStats['all'] || { count: 0, hours: 0, miles: 0 };
              $scope.missions.yearlyStats['all'].count++;
              $scope.missions.yearlyStats['all'].hours += data[i].hours;
              $scope.missions.yearlyStats['all'].miles += data[i].miles;
            }
          })
        },
        filterList: function (value) {
          var start = value.event.start;
          return $scope.missions.showYear == "all" || start.substring(0, 4) == $scope.missions.showYear;
        }
      }
    })

    $scope.missions.getList();
  }]);