angular.module('sar-database').controller("CourseDetailCtrl", ['$stateParams', '$state', '$scope', 'editorsService', 'trainingService', function ($stateParams, $state, $scope, Editors, Training) {
  refresh = function Courses_Refresh() {
    Training.courses.one($stateParams.id).get().then(function(data) {
      angular.extend($scope.course, data)
    })
    Training.courseStats($stateParams.id).then(function (stats) { angular.extend($scope.stats, stats) });
  }
  angular.extend($scope, {
    course: {},
    stats: {},
    editCourse: function (ev, course) {
      Editors
        .doEditDialog(ev, '/wwwroot/partials/training/edit-course.html', 'training course', course)
        .then(function () { refresh(); });
    },
    deleteCourse: function (ev, course) {
      Editors
        .doDelete(ev, 'training course', course)
        .then(function () { $state.go('training_courselist'); });
    }
  });
  refresh();
}]);