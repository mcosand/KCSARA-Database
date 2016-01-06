define('jquery', [], function () {
  return $;
});
define('angular', [], function () {
  return window.angular;
});

require.config({
  baseUrl: window.appRoot + 'js',
  paths: {
    'adminLTE': '../lib/AdminLTE/dist/js/app',
    'bootstrap': '../lib/bootstrap/dist/js/bootstrap',
    'moment': '../lib/moment/min/moment.min',
    'ngMaterial': '../lib/angular-material/angular-material.min',
    'ng-file-upload': '../lib/ng-file-upload/ng-file-upload',
    'np-autocomplete': '../lib/np-autocomplete/dist/np-autocomplete.min',
    'ui-bootstrap-modal': '../lib/ui.bootstrap/src/modal/modal'
  },
  shim: {
    'adminLTE': {
      deps: ['bootstrap']
    },
    'ngMaterial': {
      deps: ['../lib/angular-animate/angular-animate.min', '../lib/angular-aria/angular-aria.min', '../lib/angular-messages/angular-messages.min']
    },
    'ng-file-upload': {
      deps: ['../lib/ng-file-upload/ng-file-upload-shim']
    },
    'ui-bootstrap-modal': {
      deps: ['../lib/ui.bootstrap/src/stackedMap/stackedMap']
    }
  },
  deps: [window.appRoot + 'js/start-app.js']
})