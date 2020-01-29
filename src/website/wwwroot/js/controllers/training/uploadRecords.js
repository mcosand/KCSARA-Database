angular.module('sar-database').controller("TrainingUploadRecordsCtrl", ['$scope', '$http', '$timeout', '$q', '$mdToast', 'Upload', function ($scope, $http, $timeout, $q, $mdToast, Upload) {
  angular.extend($scope, {
    parsed: [],
    selected: function selected() {
      return $scope.parsed.filter(function (item) { return item.selected });
    },
    upload: function upload(file) {
      if (file && !file.$error) {
        $scope.uploading = true;
        $scope.progress = Upload.upload({
          url: 'https://kcsara-api2.azurewebsites.net/trainingrecords/ParseKcsaraCsv',
          data: {
            file: file
          }
        }).then(function (resp) {
          $timeout(function () {
            $scope.uploading = false;
            for (var i = 0; i < resp.data.length; i++) { resp.data[i].selected = !resp.data[i].error && !resp.data[i].existing; resp.data[i].completed = moment(resp.data[i].completed) }
            $scope.parsed = resp.data;
            $scope.log = 'file: ' +
            resp.config.data.file.name +
            ', Response: ' + JSON.stringify(resp.data) +
            '\n' + $scope.log;
          });
        }, function (err) {
          $scope.uploading = false;
          $mdToast.show($mdToast.simple().textContent('Error uploading file').position('top right').hideDelay(3000));
          return $q.reject(err);
        }, function (evt) {
          var progressPercentage = parseInt(100.0 *
              evt.loaded / evt.total);
          $scope.log = 'progress: ' + progressPercentage +
            '% ' + evt.config.data.file.name + '\n' +
            $scope.log;
        });
      }
    },
    apply: function apply() {
      var list = $scope.selected();
      list.reduce(function (promise, item) {
        return promise['finally'](function (resp) {
          var me = item;
          $scope.working = item;
          return $http({
            method: 'POST',
            url: 'https://kcsara-api2.azurewebsites.net/trainingrecords',
            data: { member: me.member, course: { id: me.course.id, name: me.course.name }, completed: me.completed, comments: me.link }
          })
          .then(function (resp) {
            delete me.error;
            me.existing = resp.data.referenceId
            delete $scope.working;
          }, function (err) {
            me.error = JSON.stringify(err.data);
            delete $scope.working;
          })
        })
      }, $timeout())
    }
  });
}]);