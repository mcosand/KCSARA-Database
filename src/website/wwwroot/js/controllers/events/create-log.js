define(['angular', 'controllers/events/base-editor'], function (angular, BaseEditor) {
  BaseEditor("CreateLogCtrl",
    function (modal, service) { return service.saveLog(modal.model, modal.dialog.eventRoute); },
    function (id, service, modal) { return service.deleteLog(modal.model.eventId, id, modal.dialog.eventRoute); }
  );

  //angular.module('sarDatabase').controller('CreateLogCtrl', ['$uibModalInstance', 'dialog', 'model', 'EventsService', function ($uibModalInstance, dialog, model, EventsService) {
  //  var modal = this;
  //  modal.dismiss = function (reason) {
  //    $uibModalInstance.dismiss(reason);
  //  }
  //  modal.submit = function () {
  //    if (!modal.createForm.$valid) return;

  //    modal.errors = {}; //clean up previous server errors

  //    EventsService.saveLog(modal.model, dialog.eventRoute)
  //    .then(function () {
  //      $uibModalInstance.close(modal.model);
  //    }, function (response) {
  //      angular.forEach(response.errors, function (errors, field) {
  //        if (modal.createForm[field] !== undefined) { modal.createForm[field].$setValidity('server', false); }
  //        modal.errors[field] = errors;
  //      });
  //    });
  //  }
  //  modal.dialog = dialog;
  //  modal.model = model;
  //}]);
});