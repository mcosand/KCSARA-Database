define(['knockout', 'site/utils'], function (ko, utils) {
  return function () {
    var self = this;
    this.username = ko.observable();
    this.firstname = ko.observable();
    this.isAuthenticated = ko.observable();
    utils.addKOLoader(this);
  }
});