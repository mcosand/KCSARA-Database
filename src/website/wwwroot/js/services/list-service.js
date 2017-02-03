angular.module('sar-database')

.provider('listService', function ListServiceProvider() {
  this.$get = ['$rootScope', '$http', 'Restangular', 'authService', function ListServiceFactory($rootScope, $http, Restangular) {
    return {
      loader: function createLoader(restangularList, opts) {
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
          },
          showAll: function () {
            delete loader.limit;
          }
        }
        if (!opts.lazy) loader.getList()
        return loader
      }
    }
  }]
})