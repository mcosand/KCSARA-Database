define(['angular', 'np-autocomplete', 'sarDatabase'], function (angular) {
  angular.module('sarDatabase').controller('MainSearchCtrl', ['$scope',
    function ($scope) {
      $.extend($scope, {
        searchOptions: {
          url: window.appRoot + 'api/search/',
          searchParam: 'q',
          itemTemplateUrl: 'searchResult.html',
          listClass: 'list-group search-results',
          limit: 8,
          noFixWidth: true,
          onSelect: function (item) {
            if (item.type == 'Member') {
              window.location.href = window.appRoot + 'Members/Detail/' + item.summary.id;
            }
            else if (item.type == "Mission") {
              window.location.href = window.appRoot + 'Missions/Roster/' + item.summary.id;
            }
            else {
              console.log(item);
            }
          }
          // np-autocomplete.min.js modified to remove assignment of width
        }
      });
    }]);
})