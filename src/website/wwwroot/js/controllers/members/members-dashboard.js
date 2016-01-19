angular.module('sarDatabase').controller('MembersDashboardCtrl', ['$scope',
  function ($scope) {
    $.extend($scope, {
      searchOptions: {
        url: window.appRoot + 'api/search/',
        searchParam: 'q',
        itemTemplateUrl: 'searchResult.html',
        listClass: 'list-group search-results',
        limit: 8,
        params: {t: 'Member'},
        onSelect: function (item) {
          window.location.href = window.appRoot + 'Members/' + item.summary.id;
        }
      }
    })
  }]);