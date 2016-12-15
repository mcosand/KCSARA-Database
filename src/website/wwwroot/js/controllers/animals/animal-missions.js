angular.module('sar-database').controller("AnimalsMissionsCtrl", ['$stateParams', '$window', '$scope', '$rootScope', 'animalsService',
  function ($stateParams, $window, $scope, $rootScope, Animals) {
    angular.extend($scope, {
      stats: Animals.animals.one($stateParams.id).all('missions').one('stats').get().$object,
      missions: {
        query: {
          order: '-event.startTime'
        },
        list: [],
        years: [],
        yearlyStats: {},
        showYear: null,
        getList: function () {
          $scope.missions.loading = Animals.animals.one($stateParams.id).all('missions').getList().then(function (data) {
            $scope.missions.list = data;
            $scope.missions.years = [];
            $scope.missions.yearlyStats = {};
            var year = null;
            for (var i = 0; i < data.length; i++) {
              var y = data[i].event.start.substring(0, 4);
              if (y != year) $scope.missions.years.push(y);
              if (!$scope.missions.showYear) $scope.missions.showYear = y;
              year = y;
              $scope.missions.yearlyStats[y] = $scope.missions.yearlyStats[y] || { count: 0, hours: 0 };
              $scope.missions.yearlyStats[y].count++;
              $scope.missions.yearlyStats[y].hours += data[i].hours;
            }
          })
        },
        filterList: function (value) {
          var start = value.event.start;
          return start.substring(0, 4) == $scope.missions.showYear;
        }
      },
      gotoMission: function (mission) {
        $window.location.href = '/missions/roster/' + mission.event.id;
        //$window.location.reload();
      }
    })

    $scope.missions.getList();
  }]);