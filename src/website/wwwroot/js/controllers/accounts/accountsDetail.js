angular.module('sar-database').controller("AccountsDetailCtrl", [
  '$stateParams', '$scope', '$http', '$mdDialog', '$mdToast', 'editorsService', 'accountsService', 'currentUser',
  function ($stateParams, $scope, $http, $mdDialog, $mdToast, Editors, Accounts, CurrentUser) {
    var accountId = $stateParams.id || CurrentUser.user.profile.sub;
    var refresh = function () {
      $scope.account = Accounts.accounts.one(accountId).get().$object;
    }

    angular.extend($scope, {
      account: null,
      isMe: function() {
        return $scope.account.id === CurrentUser.user.profile.sub;
      },
      resetPassword: function resetPassword(ev) {
        var confirm = $mdDialog.confirm()
                                .title('Trigger password reset')
                                .textContent('Send email to ' + $scope.account.email + ' with password reset code?')
                                .ariaLabel('Trigger password reset for ' + $scope.account.firstName + ' ' + $scope.account.lastName)
                                .targetEvent(ev)
                                .ok('Reset')
                                .cancel('Cancel');
        return $mdDialog.show(confirm).then(function () {
          return $http.post('/api2/accounts/' + $scope.account.id + '/resetpassword');
        }).then(function () {
          $mdToast.show($mdToast.simple().textContent('Sent reset code to ' + $scope.account.email).position('top right').hideDelay(3000));
        });
      },
      setPassword: function setPassword(ev) {
        var item = { username: $scope.account.username };
        return $mdDialog.show({
          controller: 'EditDialogCtrl',
          locals: {
            title: ('Set Password'),
            item: item,
            saveMethod: function (formScope, newValues) {
              return $http.post('/api2/accounts/' + $scope.account.id + '/password', JSON.stringify(formScope.password));
            }
          },
          targetEvent: ev,
          templateUrl: '/partials/accounts/set-password.html'
        })
        .then(function (statusValues) {
          $mdToast.show($mdToast.simple().textContent('Password updated for ' + $scope.account.username).position('top right').hideDelay(3000));
          return statusValues;
        });
      },
      unlock: function ($event) {
        return $mdDialog.show($mdDialog.confirm()
          .title('Unlock Account')
          .textContent('Unlock the account?')
          .targetEvent($event)
          .ok('Unlock')
          .cancel('Cancel'))
        .then(function () {
          return $http.post('/api2/accounts/' + $scope.account.id + '/unlock')
          .then(function () {
            refresh()
            $mdToast.show($mdToast.simple().textContent('Account unlocked').position('top right').hideDelay(3000));
          })
        })
      },
      lock: function ($event) {
        return $mdDialog.show($mdDialog.prompt()
          .title('Lock Account')
          .textContent('You are about to prevent this user from being able to log on. Why is this action being taken?')
          .targetEvent($event)
          .placeholder('Lock reason')
          .ok('Lock')
          .cancel('Cancel'))
        .then(function (lockReason) {
          if (!lockReason || lockReason === '')
          {
            alert('No reason given - not locking account');
            return;
          }
          return $http.post('/api2/accounts/' + $scope.account.id + '/lock', JSON.stringify(lockReason))
          .then(function () {
            refresh()
            $mdToast.show($mdToast.simple().textContent('Account locked').position('top right').hideDelay(3000));
          })
        })
      },
      edit: function () { alert('edit') }
    });
    refresh();
  }]);