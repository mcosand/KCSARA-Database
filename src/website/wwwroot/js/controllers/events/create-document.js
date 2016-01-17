CreateEditorController("CreateDocCtrl",
  function (modal, service) { return service.saveDocument(modal.model, modal.dialog.eventRoute); },
  function (id, service) { return service.deleteDocument(id); },
  function (modal) {
    return {
      download: function () {
        window.open(modal.model.url, '_blank', '');
      }
    }
  });
