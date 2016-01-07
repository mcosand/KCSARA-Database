define(['angular'], function (angular) {
  return function(controllerName, saveAction, deleteAction, extend) {
    angular.module('sarDatabase').controller(controllerName, ['$uibModalInstance', 'dialog', 'model', 'EventsService', function ($uibModalInstance, dialog, model, EventsService) {
      var modal = this;
      $.extend(modal, {
        dismiss: function (reason) {
          $uibModalInstance.dismiss(reason);
        },
        submit: function () {
          if (!modal.createForm.$valid) return;

          modal.isSaving = true;
          modal.isWorking = true;
          modal.errors = {}; //clean up previous server errors
          
          saveAction(modal, EventsService)
          .then(function () {
            delete modal.isSaving;
            delete modal.isWorking;
            $uibModalInstance.close(modal.model);
          }, function (response) {
            angular.forEach(response.errors, function (errors, field) {
              if (modal.createForm[field] !== undefined) { modal.createForm[field].$setValidity('server', false); }
              modal.errors[field] = errors;
            });
            delete modal.isSaving;
            delete modal.isWorking;
          });
        },
        checkDelete: function(name) {
          if (confirm("Delete " + name + "?")) {
            modal.isDeleting = true;
            modal.isWorking = true;
            deleteAction(modal.model.id, EventsService, modal)
            .then(function () {
              delete modal.isDeleting;
              delete modal.isWorking;
              $uibModalInstance.close(modal.model);
            }, function (msg) {
              alert("Error: " + msg);
              delete modal.isDeleting;
              delete modal.isWorking;
            });
          }
        },
        dialog: dialog,
        model: model
      }, (extend || function() { })(modal));
    }]);
  };
});