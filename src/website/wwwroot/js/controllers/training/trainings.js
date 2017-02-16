angular.module('sar-database').controller("TrainingsCtrl", ['$stateParams', 'listService', 'trainingService', 'editorsService',
  function ($stateParams, Lists, Training, Editors) {
    var self = this;
    angular.extend(this, {
      trainings: Lists.pagedEventsLoader(Training.years, Training.trainings, { miles: true, persons: true, showDelete: true }),
      actions: {
        deleteEvent: function (ev, training) {
          Editors
            .doDelete(ev, 'training', training, { getName: function (t) { return t.event.name } })
            .then(function () { self.trainings.getPages(); });
        }
      }
    })
  }]);