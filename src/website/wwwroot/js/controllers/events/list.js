function scrollToTop(callback) {
  if ($('html').scrollTop()) {
    $('html').animate({ scrollTop: 0 }, { duration: 200, complete: callback });
    return;
  }

  if ($('body').scrollTop()) {
    $('body').animate({ scrollTop: 0 }, { duration: 200, complete: callback });
    return;
  }

  callback();
}

angular.module('sarDatabase').controller('EventListCtrl', ['$scope', '$rootScope', '$location', '$uibModal', 'EventsService',
    function ($scope, $rootScope, $location, $uibModal, EventsService) {
      console.log('page loads with location: ' + ($location.path() || 'null'));

      var loadList = function (year, eventRoute) {
        EventsService.list($scope.list, eventRoute, year);
        $location.path(year);
      }

      function yearFromHash() {
        var match = ($location.path() || "").match(/\d{4}/)
        return match ? (match[0] * 1) : null;
      }

      $.extend($scope, {
        list: [],
        years: [],
        year: yearFromHash(),
        detailedEvent: null,
        eventMap: null,
        eventTypeText: '',
        eventRoute: 'unknown',
        init: function (eventRoute, eventTypeText) {
          $scope.eventRoute = eventRoute;
          $scope.eventTypeText = eventTypeText;
          if ($scope.year) {
            loadList($scope.year, $scope.eventRoute)
          }
          EventsService.years($scope.years, $scope.eventRoute).then(function (list) {
            if (!$scope.year && list.length > 0) { $scope.year = list[0]; console.log('setting year in years callback: ' + ($scope.year || 'null')) }
          });
        },
        showInfo: function (e) {
          $scope.detailedEvent = ($scope.detailedEvent == e) ? null : e;
          if ($scope.detailedEvent && $scope.detailedEvent.baseLocation) {
            window.setTimeout(function () {
              $scope.eventMap = L.map('eventMap', { zoomControl: false, attributionControl: false, dragging: false, touchZoom: false, scrollWheelZoom: false, doubleClickZoom: false, boxZoom: false })
                .setView([$scope.detailedEvent.baseLocation.lat, $scope.detailedEvent.baseLocation.lng], 10);
              L.kcsarStaticMap($scope.eventMap, { showLayers: false, defaultBase: 'OpenStreetMap' });
            }, 5);
          }
        },
        tabLink: function (action, id) {
          return window.appRoot + $scope.eventRoute + '/' + id + '#/' + action;
        },
        mapClick: function (id, $event) {
          alert('hi');
          $event.stopPropagation();
        },
        startCreate: function () {
          var parts = $location.path().split('/');
          if (parts.length < 1) { parts.push(''); }
          if (parts.length < 2) { parts.push($scope.year); }
          if (parts.length < 3) { parts.push('create'); }
          parts[2] = "create";
          $location.path(parts.join('/'));
        },
        showCreateDialog: function () {
          scrollToTop(function () {
            var model = {
              start: moment(),
              startText: function (val) {
                if (arguments.length && val) {
                  model.start = moment(val, ["YYYY-MM-DD HH:mm", "YYYY-MM-DD HHmm", "MM/DD/YYYY HH:mm", "MM/DD/YYYY HHmm"]);
                  model.start.toJSON = function () { return model.start.format("YYYY-MM-DDTHH:mm") };
                  return model.start;
                } else if (arguments.length) {
                  model.start = null;
                } else {
                  return model.start.format('YYYY-MM-DD HH:mm');
                }
              },
              county: 'king'
            };
            model.start.toJSON = function () { return model.start.format("YYYY-MM-DDTHH:mm") };

            var modalInstance = $uibModal.open({
              templateUrl: window.appRoot + 'partials/events/create.html',
              controller: 'CreateEventCtrl',
              controllerAs: 'modal',
              size: 'lg',
              resolve: {
                model: function () { return model },
                dialog: function () {
                  return {
                    eventType: $scope.eventTypeText,
                    eventRoute: $scope.eventRoute
                  }
                }
              }
            });
            modalInstance.result
            .then(function (data) {
              loadList(data.start.year(), $scope.eventRoute);
              $location.path($location.path().toUpperCase().replace('/CREATE', ''));
            }, function (reason) {
              $location.path($location.path().toUpperCase().replace('/CREATE', ''));
            });
          });
        },
        can: {
          create: true//@(User.IsInRole("cdb." + ViewBag.EventTypeText.ToLower() + "editors").ToString().ToLower())
        }
      });
      $scope.$watch("year", function (newValue, oldValue) {
        if (newValue == oldValue) return;
        console.log('updating list from year watcher. Newvalue:' + newValue);
        loadList(newValue, $scope.eventRoute);
      });
      $rootScope.$on('$locationChangeStart', function (event, newUrl, oldUrl) {
        console.log('handle hash change:' + $location.path());
        $scope.year = yearFromHash();
        console.log($location.path().toLowerCase().indexOf('/create'));
        if ($location.path().toLowerCase().indexOf('/create') > -1) {
          $scope.showCreateDialog();
        }
      });
    }]);