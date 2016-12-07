angular.module('sar-database').controller("CourseRosterCtrl", ['$stateParams', '$scope', 'trainingService', function ($stateParams, $scope, Training) {
  angular.extend($scope, {
    query: {
      order: 'name',
      limit: 5,
      page: 1
    },
    showExpired: false,
    doFilter: function (item) {
      return $scope.showExpired || (!item.expires || (new Date(item.expires) >= new Date()))
    },
    getRoster: function () {
      $scope.loading = Training.courses.one($stateParams.id).all('roster').getList().then(function (data) {
        $scope.roster = data;
      })
    },
    course: {},
    roster: []
  });
  $scope.getRoster()
  Training.courses.one($stateParams.id).get().then(function (course) {
    $scope.course = course;
  })
}]);