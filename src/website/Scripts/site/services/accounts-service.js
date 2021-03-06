﻿angular.module('sarDatabase').service('AccountService', ['$http', '$q', function ($http, $q) {
  self = this;

  $.extend(this, {
    accounts: {},
    groups: {},
    initAccount: function (model) {
      var accountModel;
      if (model['name']) {
        accountModel = new DatabaseAccountModel(model);
        self.accounts[model.name] = accountModel;
      }
      return accountModel;
    },
    loadGroups: function (username) {
      var accountModel = self.accounts[username];

      var deferred = $q.defer();
      if (accountModel.groups.loaded) {
        deferred.resolve(accountModel.groups);
        return deferred;
      }

      accountModel.groups.length = 0;
      accountModel.groups.loading = true;

      $http({
        method: 'GET',
        url: window.appRoot + 'api/account/rolesforuser/' + username,
      }).success(function (data) {
        $.each(data, function (idx, group) {
          if (!self.groups[group]) {
            self.groups[group] = { name: group, groups: [], canEdit: self.rolesIManage.indexOf(group) >= 0 };
            self.groups[group].groups.loaded = false;
            accountModel.groups.push($.extend({ collapsed: true, remove: false }, self.groups[group]));
          }
        });
        delete accountModel.groups.loading;
        accountModel.groups.loaded = true;
        deferred.resolve(data);
      })
      .error(function (response) { deferred.reject(response); });
    },
    loadMoreGroups: function (group) {
      var groupModel = self.groups[group.name];
      var deferred = $q.defer();
      if (groupModel.groups.loaded) {
        deferred.resolve();
        return deferred.promise;
      }

      $http({
        method: 'GET',
        url: window.appRoot + 'api/account/rolesforrole/' + group.name
      }).success(function (data) {
        $.each(data, function (idx, group) {
          if (!self.groups[group]) {
            self.groups[group] = { name: group, groups: [] };
            self.groups[group].groups.loaded = false;
          }
          groupModel.groups.push($.extend({ collapsed: true, remove: false }, self.groups[group]));
        })
        groupModel.groups.loaded = true;
      })
      return deferred.promise;
    },
    loadLinkedMember: function (username) {
      var accountModel = self.accounts[username];
      var deferred = $q.defer();
      if (accountModel.linkedMember.loaded) {
        deferred.resolve(accountModel.linkedMember);
        return deferred.promise;
      }
      accountModel.linkedMember.loading = true;
      $http({
        method: 'GET',
        url: window.appRoot + 'api/members/byusername/' + username
      }).success(function (data) {
        if (data.length == 1) {
          accountModel.linkedMember.id = data[0].Id;
          accountModel.linkedMember.name = data[0].Name;
          accountModel.linkedMember.units = data[0].Units;
          accountModel.linkedMember.dem = data[0].DEM;
        }
        delete accountModel.linkedMember.loading;
        accountModel.linkedMember.loaded = true;
        deferred.resolve(data);
      })
      .error(function (response) { deferred.reject(response); });
      return deferred.promise;
    },
    getMemberAccount: function(userId) {
      var deferred = $q.defer();
      $http({
        method: 'GET',
        url: window.appRoot + 'api/members/accountfor/' + userId
      })
      .success(function (data) { deferred.resolve(data); })
      .error(function (response) { deferred.reject(response); });
      return deferred.promise;
    },
    /*
     *
     */
    save: function (model) {
      var deferred = $q.defer();
      if (model.linkedMember.loaded) delete model.linkedMember.loaded;
      $http({
        method: 'PUT',
        url: window.appRoot + 'api/account',
        data: model,
      })
      .success(function (data) { deferred.resolve(data); })
      .error(function (response) { deferred.reject(response); })
      return deferred.promise;
    },
    setPassword: function (model) {
      if (model.type == 'set') {
        return self.save($.extend({}, model.account, { password: model.password }));
      }
      var deferred = $q.defer();
      $http({
        method: 'POST',
        url: window.appRoot + 'api/account/resetpassword/' + model.account.name
      })
      .success(function (data) { deferred.resolve(); })
      .error(function (response) { deferred.reject(response); })
      return deferred.promise;
    }
  });
}]);