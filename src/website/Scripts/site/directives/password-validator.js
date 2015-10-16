angular.module('sarDatabase').directive('validatePasswordCharacters', function () {

  var REQUIRED_PATTERNS = [
    /\d+/,    //numeric values
    /[a-z]+/, //lowercase values
    /[A-Z]+/, //uppercase values
    /\W+/,    //special characters
    /^\S+$/   //no whitespace allowed
  ];

  return {
    require: 'ngModel',
    link: function ($scope, element, attrs, ngModel) {
      ngModel.$validators.passwordCharacters = function (value) {
        if (!value) {
          return true;
        }
        var status = true;
        angular.forEach(REQUIRED_PATTERNS, function (pattern) {
          status = status && pattern.test(value);
        });
        return status;
      };
    }
  }
});
