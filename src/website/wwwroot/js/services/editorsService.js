angular.module('sar-database')

.provider('editorsService', function EditorsServiceProvider() {
  function defaultNameResolver(item) { return item.name }

  this.$get = ['$mdDialog', '$mdToast', '$q', function EditorsServiceFactory($mdDialog, $mdToast, $q) {
    return {
      doDelete: function doDelete(ev, itemType, item, more) {
        var namer = (more || {}).nameResolver || defaultNameResolver;

        var confirm = $mdDialog.confirm()
                                .title('Delete ' + itemType)
                                .textContent('Are you sure you want to delete ' + itemType + ': ' + namer(item) + '?')
                                .ariaLabel('Delete ' + itemType + ' ' + namer(item))
                                .targetEvent(ev)
                                .ok('Delete')
                                .cancel('Cancel');
        return $mdDialog.show(confirm).then(function () {
          return item.remove();
        }).then(function () {
          $mdToast.show($mdToast.simple().textContent('Removed ' + itemType + ' - ' + namer(item)).position('top right').hideDelay(3000));
        });
      },
      doEditDialog: function doEditDialog(ev, templateUrl, itemType, item, more) {
        var namer = (more || {}).nameResolver || defaultNameResolver;
        var fromServer = item.fromServer;
        item = item.clone(); // clone doesn't seem to keep .fromServer, which affects .save()
        item.fromServer = fromServer;
        return $mdDialog.show({
          controller: 'EditDialogCtrl',
          locals: {
            title: (item.id ? 'Edit ' : 'Add new ') + itemType + (item.id ? ' - ' + namer(item) : ''),
            item: item.plain(),
            more: more,
            saveMethod: function (formScope, newValues) {
              angular.extend(item, newValues);
              return item.save()
                .catch(
                  function (response) {
                    if (response.status == 400) {
                      console.log('merge errors');

                      var message = JSON.parse(response.data.message);
                      for (var p in formScope.editUnitStatus) {
                        if (formScope.editUnitStatus.hasOwnProperty(p) && p[0] != '$') {
                          console.log(p);
                          error = message.errors[p];
                          formScope.editUnitStatus[p].$setValidity("server", !error);
                          formScope.editUnitStatus[p].$error.server = (error || []).join();
                        }
                      }
                    }
                    return $q.reject(response);
                  }
                );
            }
          },
          targetEvent: ev,
          templateUrl: templateUrl
        })
        .then(function (statusValues) {
          $mdToast.show($mdToast.simple().textContent('Saved ' + itemType + ' - ' + namer(statusValues)).position('top right').hideDelay(3000));
          return statusValues;
        });
      }
    };
  }]
});