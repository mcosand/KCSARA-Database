CreateEditorController("CreateLogCtrl",
    function (modal, service) { return service.saveLog(modal.model, modal.dialog.eventRoute); },
    function (id, service, modal) { return service.deleteLog(modal.model.eventId, id, modal.dialog.eventRoute); }
  );