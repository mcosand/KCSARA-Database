angular.module('sar-database').controller("MembersTrainingCtrl", ['$stateParams', '$window', '$scope', '$rootScope', 'editorsService', 'listService', 'membersService',
  function ($stateParams, $window, $scope, $rootScope, Editors, Lists, Members) {
    var restMember = Members.members.one($stateParams.id)
    angular.extend($scope, {
      memberId: $stateParams.id,
      stats: restMember.all('trainings').one('stats').get().$object,
      trainings: {
        query: {
          order: '-event.startTime'
        },
        list: [],
        years: [],
        yearlyStats: {},
        showYear: null,
        getList: function () {
          $scope.trainings.loading = restMember.all('trainings').getList().then(function (data) {
            $scope.trainings.list = data;
            $scope.trainings.years = [];
            $scope.trainings.yearlyStats = {};
            var year = null;
            for (var i = 0; i < data.length; i++) {
              var y = data[i].event.start.substring(0, 4);
              if (y != year) $scope.trainings.years.push(y);
              if (!$scope.trainings.showYear) $scope.trainings.showYear = y;
              year = y;
              $scope.trainings.yearlyStats[y] = $scope.trainings.yearlyStats[y] || { count: 0, hours: 0, miles: 0 };
              $scope.trainings.yearlyStats[y].count++;
              $scope.trainings.yearlyStats[y].hours += data[i].hours;
              $scope.trainings.yearlyStats[y].miles += data[i].miles;
              $scope.trainings.yearlyStats['all'] = $scope.trainings.yearlyStats['all'] || { count: 0, hours: 0, miles: 0 };
              $scope.trainings.yearlyStats['all'].count++;
              $scope.trainings.yearlyStats['all'].hours += data[i].hours;
              $scope.trainings.yearlyStats['all'].miles += data[i].miles;
            }
          })
        },
        filterList: function (value) {
          var start = value.event.start;
          return $scope.trainings.showYear == "all" || start.substring(0, 4) == $scope.trainings.showYear;
        }
      },
      required: Lists.loader(restMember.all('requiredtraining'), { transform: function(data) {
            return data.reduce(function (accum, item) {
              accum[item.course.name] = item; return accum;
            })
      }}),
      recent: Lists.loader(restMember.all('trainingrecords'), { limit: 15, order: '-completed' }),
      showRecord: function showRecord($event, recordId) {
        $event.preventDefault();
        Editors.doPopup($event, 'Training Details', '/wwwroot/partials/training/record.html', { memberId: $scope.memberId, recordId: recordId })
      }
    })
    $scope.trainings.getList()
  }]);