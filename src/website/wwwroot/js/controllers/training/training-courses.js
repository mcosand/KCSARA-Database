angular.module('sar-database').controller("TrainingCoursesCtrl", ['$scope', '$rootScope', '$state', 'trainingService', 'editorsService', function ($scope, $rootScope, $state, Training, Editors) {
  refreshCourses = function Courses_Refresh() {
    $scope.courses.loading = Training.courses.getList().then(function (list) { $scope.courses = list });
  }

  angular.extend($scope, {
    courses: [],
    createCourse: function (ev) {
      var course = Training.createCourse();
      Editors
        .doEditDialog(ev, '/wwwroot/partials/training/edit-course.html', 'training course', course)
        .then(function (data) { $state.go('training_coursedetail', { id: data.id }); });
    }
  });
  refreshCourses();
}]);