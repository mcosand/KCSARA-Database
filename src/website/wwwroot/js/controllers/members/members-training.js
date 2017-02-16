angular.module('sar-database').controller("MembersTrainingCtrl", ['$stateParams', '$window', '$scope', '$rootScope', 'editorsService', 'listService', 'membersService',
  function ($stateParams, $window, $scope, $rootScope, Editors, Lists, Members) {
    var restMember = Members.members.one($stateParams.id)
    angular.extend($scope, {
      memberId: $stateParams.id,
      stats: restMember.all('trainings').one('stats').get().$object,
      trainings: Lists.eventsLoader(Members.members.one($stateParams.id).all('trainings'), { miles: true }),
      recent: Lists.loader(restMember.all('trainingrecords'), { limit: 15, order: '-completed' }),
      required: Lists.loader(restMember.all('requiredtraining'), {
        transform: function (data) {
            return data.reduce(function (accum, item) {
              accum[item.course.name] = item; return accum;
            }, {})
      }}),
      showRecord: function showRecord($event, recordId) {
        $event.preventDefault();
        Editors.doPopup($event, 'Training Details', '/wwwroot/partials/training/record.html', { memberId: $scope.memberId, recordId: recordId })
      }
    })
  }]);