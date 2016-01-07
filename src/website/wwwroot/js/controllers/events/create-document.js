define(['angular', 'controllers/events/base-editor', 'ng-file-upload'], function (angular, BaseEditor) {
  BaseEditor("CreateDocCtrl",
    function (modal, service) { return service.saveDocument(modal.model, modal.dialog.eventRoute); },
    function (id, service) { return service.deleteDocument(id); },
    function (modal) {
      return {
        download: function () {
          window.open(modal.model.url, '_blank', '');
        }
      }
    });

  //angular.module('sarDatabase').controller('CreateDocCtrl', ['$uibModalInstance', 'dialog', 'model', 'EventsService', function ($uibModalInstance, dialog, model, EventsService) {
  //  var modal = this;
  //  $.extend(modal, {
  //    dismiss: function (reason) {
  //      $uibModalInstance.dismiss(reason);
  //    },
  //    submit: function () {
  //      if (!modal.createForm.$valid) return;

  //      modal.isSaving = true;
  //      modal.errors = {}; //clean up previous server errors

  //      EventsService.saveDocument(modal.model, dialog.eventRoute)
  //      .then(function () {
  //        delete modal.isSaving;
  //        $uibModalInstance.close(modal.model);
  //      }, function (response) {
  //        angular.forEach(response.errors, function (errors, field) {
  //          if (modal.createForm[field] !== undefined) { modal.createForm[field].$setValidity('server', false); }
  //          modal.errors[field] = errors;
  //        });
  //        delete modal.isSaving;
  //      });
  //    },
  //    download: function () {
  //      window.open(model.url, '_blank', '');
  //    },
  //    dialog: dialog,
  //    model: model
  //  });
  //}]);
});