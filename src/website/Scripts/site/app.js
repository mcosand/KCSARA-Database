/*
 * Copyright 2015 Matthew Cosand
 */
define(['knockout', 'site/user', 'site/components/searchbox', 'site/components/login-form'], function(ko, UserModel) {
  return function () {
    var self = this;
    this.page = null;
    this.searchResult = ko.observable();
    this.searchResult.subscribe(function (newValue) {
      window.location.href = newValue.url;
    })
    this.user = ko.observable(new UserModel());
  };
})