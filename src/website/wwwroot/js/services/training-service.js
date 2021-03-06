﻿angular.module('sar-database')

.provider('trainingService', function TrainingServiceProvider() {
  this.$get = ['$rootScope', '$http', 'Restangular', 'authService', function TrainingServiceFactory($rootScope,$http, Restangular) {
    var result = {
      courses: Restangular.service('training/courses'),
      createCourse: function () { return Restangular.restangularizeElement(null, { category: 'other' }, '/training/courses') },
      courseStats: function (courseId) {
        return $http.get('https://kcsara-api2.azurewebsites.net/training/courses/' + courseId + '/stats').then(function (msg) {
          return msg.data;
        });
      }
    };

    return result;
  }]
});