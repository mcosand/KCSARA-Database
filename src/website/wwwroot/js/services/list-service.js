angular.module('sar-database')

.provider('listService', function ListServiceProvider() {
  this.$get = ['$rootScope', '$http', 'Restangular', 'authService', function ListServiceFactory($rootScope, $http, Restangular) {
    var service;
    service = {
      loader: function loader(restangularList, opts) {
        opts = opts || {}
        var loader = {
          list: [],
          limit: opts.limit,
          loading: false,
          query: {
            order: opts.order
          },
          getList: function() {
            loader.loading = restangularList.getList().then(function (data) {
              loader.list = opts.transform ? opts.transform(data, loader) : data;
              loader.loading = false
            })
            return loader.loading
          },
          showAll: function () {
            delete loader.limit;
          }
        }
        if (!opts.lazy) loader.getList()
        return loader
      },
      pagedEventsLoader: function(pagesService, listService, opts) {
        opts = opts || {}
        opts.order = opts.order || '-event.start'
        var loader = {
          years: [],
          list: [],
          loading: false,
          query: {
            order: opts.order
          },
          getPages: function () {
            loader.loading = pagesService.getList().then(function (pages) {
              loader.years = []
              for (var i = 1; i < pages.length; i++) { loader.years.push(pages[i].year) }
              loader.showYear = pages[1].year
              return loader.getYear()
            })
          },
          getYear: function () {
            loader.loading = listService.getList(loader.showYear === 'all' ? null : { year: loader.showYear }).then(function (data) {
              loader.list = data
            })
            return loader.loading
          }
        }
        if (opts.miles) loader.showMiles = true
        if (opts.persons) loader.showPersons = true
        if (opts.showDelete) loader.showDelete = true
        if (!opts.lazy) loader.getPages();       
        return loader;
      },
      eventsLoader: function eventsLoader(restangularList, opts) {
        opts = opts || {}
        opts.order = opts.order || '-event.start'
        opts.transform = function (data, loader) {
          loader.list = data;
          loader.years = [];
          loader.yearlyStats = {};
          var year = null;
          var statsTemplate = { count: 0, hours: 0 }
          if (opts.miles) statsTemplate.miles = 0
          for (var i = 0; i < data.length; i++) {
            var y = data[i].event.start.substring(0, 4);
            if (y !== year) loader.years.push(y);
            if (!loader.showYear) loader.showYear = y;
            year = y;
            loader.yearlyStats[y] = loader.yearlyStats[y] || JSON.parse(JSON.stringify(statsTemplate))
            loader.yearlyStats[y].count++
            loader.yearlyStats[y].hours += data[i].hours
            loader.yearlyStats['all'] = loader.yearlyStats['all'] || JSON.parse(JSON.stringify(statsTemplate))
            loader.yearlyStats['all'].count++
            loader.yearlyStats['all'].hours += data[i].hours

            if (opts.miles) {
              loader.yearlyStats[y].miles += data[i].miles
              loader.yearlyStats['all'].miles += data[i].miles
            }
          }
          return data
        }
        var list = service.loader(restangularList, opts)
        if (opts.miles) list.showMiles = true
        if (opts.persons) list.showPersons = true
        if (opts.showDelete) loader.showDelete = true

        list.filterList = function (item) {
          var start = item.event.start;
          return list.showYear === "all" || start.substring(0, 4) === list.showYear;
        }

        return list
      }
    }
    return service
  }]
})