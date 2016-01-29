/*
 * Copyright 2016 Matthew Cosand
 */
angular.module('sarDatabase').service('MembersService', ['ApiLoader', '$http', '$q', function (ApiLoader, $http, $q) {
  $.extend(this, {
    memberships: function(fillList, memberId) {
      return ApiLoader.getIntoList(
        fillList,
        window.appRoot + 'api/members/' + memberId + '/memberships',
        function(data) {
          $.each(data, function(idx, m) {
            m.start = moment(m.start);
            if (m.stop) m.stop = moment(m.stop);
            fillList.push(m);
          })
        });
    },
    medical: function (target, memberId, justification) {
      return ApiLoader.getInto(target,
        window.appRoot + 'api/members/' + memberId + '/medical',
        null);
    },
    addresses: function (fillList, memberId) {
      return ApiLoader.getIntoList(
        fillList,
        window.appRoot + 'api/members/' + memberId + '/addresses',
        null)
    },
    contacts: function (fillList, memberId) {
      return ApiLoader.getIntoList(
        fillList,
        window.appRoot + 'api/members/' + memberId + '/contacts',
        null)
    },
    events: function (fillList, memberId, eventType) {
      return ApiLoader.getIntoList(
        fillList,
        window.appRoot + 'api/members/' + memberId + '/' + eventType,
        function (data) {
          fillList.count = fillList.hours = fillList.miles = 0;
          $.each(data, function (idx, evt) {
            evt.date = moment(evt.date);
            fillList.unshift(evt);
            fillList.count++;
            fillList.hours += evt.hours || 0;
            fillList.miles += evt.miles || 0;
          })
        })
    },
    eventTimeline: function (fillList, memberId, eventType, eventId) {
      return ApiLoader.getIntoList(
        fillList,
        window.appRoot + 'api/members/' + memberId + '/' + eventType + '/' + eventId + '/timeline',
        function (timeline) {
          $.each(timeline, function (idx, item) {
            item.time = item.time ? moment(item.time) : null;
            fillList.unshift(item);
          });
        });
    },
    eventAwards: function (fillList, memberId, eventId) {
      return ApiLoader.getIntoList(
        fillList,
        window.appRoot + 'api/members/' + memberId + '/training/event/' + eventId,
        function (data) {
          $.each(data, function (idx, award) {
            fillList.push(award);
          });
        });
    },
    latestTraining: function (fillList, memberId) {
      return ApiLoader.getIntoList(
      fillList,
      window.appRoot + 'api/members/' + memberId + '/training/latest',
      function (data) {
        $.each(data, function (idx, item) {
          if (item.completed) item.completed = moment(item.completed);
          if (item.expires) item.expires = moment(item.expires);
          fillList.unshift(item);
        });
      });
    },
    requiredTraining: function (fillList, memberId) {
      return ApiLoader.getIntoList(
      fillList,
      window.appRoot + 'api/members/' + memberId + '/training/required',
      function (data) {
        var update = function (x) {
          x.expires = x.expires ? moment(x.expires) : null;
          x.text = x.expires ? x.expires.format('YYYY\u2011MM\u2011DD') : x.status;
          x.completed = x.completed ? moment(x.completed) : null;
        }
        for (var s = 0; s < data.length; s++) {
          var cs = data[s].courses;
          for (var c = 0; c < cs.length; c++) {
            var x = cs[c];
            update(x);
            if (x.parts && x.parts.length) {
              for (var p = 0; p < x.parts.length; p++) {
                update(x.parts[p]);
              }
            }
          }
          fillList.push(data[s]);
        }
      });
    }
  });
}]);