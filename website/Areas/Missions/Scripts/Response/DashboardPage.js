var PageModel = function (responseClient, mapElement) {
  var self = this;

  var map;

  this.Missions = ko.observableArray();
  this.Missions.isLoading = ko.observable(true);

  function respondersFactory(mission) {
    return function () {
      mission.Responders.isLoading(true);
      $.ajax({ type: 'GET', url: apiRoot + '/Missions/Response/GetResponders/' + mission.Mission.Id, data: null, dataType: 'json' })
        .done(function (data) {
          for (i = 0; i < data.length; i++) {
            mapMoment(data[i], ['Updated']);
          }
          mission.Responders(data);
        })
        .always(function () {
          mission.Responders.isLoading(false);
        });
    }
  }
  function clearMarkers(oldValue) {
    console.log('drop')
    for (var i = 0; i < oldValue.length; i++) {
      if (oldValue[i]._marker) {
        oldValue[i]._marker.setMap(null);
        delete oldValue[i]._marker;
      }
    }
  }
  function updateMarkers(newValue)
  {
    for (var i = 0; i < newValue.length; i++) {
      var loc = newValue[i].Location;
      if (loc && map) {
        var pos = new google.maps.LatLng(loc.latitude, loc.longitude, 4326);
        var marker = new google.maps.Marker({ position: pos, map: map, title: 'fio' });
        //marker.setAnimation(google.maps.Animation.BOUNCE);
        newValue[i]._marker = marker;
      }
    }
    console.log(newValue);
  }

  this.loadMissions = function () {
    self.Missions.isLoading(true);
    $.ajax({ type: 'GET', url: apiRoot + '/Missions/Response/GetCurrentStatus', data: null, dataType: 'json' })
      .done(function (data) {
        for (i = 0; i < data.length; i++) {
          mapMoment(data[i].Mission, ['StartTime']);
          mapMoment(data[i], ['NextStart', 'StopStaging']);
          data[i].Responders = ko.observableArray();
          data[i].Responders.isLoading = ko.observable(true);
          data[i].Responders.Loader = respondersFactory(data[i]);
          if (map != null) {
            data[i].Responders.subscribe(clearMarkers, self, 'beforeChange');
            data[i].Responders.subscribe(updateMarkers, self);
          }
          data[i].Responders.Loader();
        }
        self.Missions(data);
      })
      .always(function () {
        self.Missions.isLoading(false);
      });
  };

  this.load = function () {
    self.loadMissions();
  }

  responseClient.notifyRespondersUpdate = function (missionId) {
    // Add the message to the page. 
    console.log(missionId);
    var list = self.Missions();
    for (var i = 0; i < list.length; i++) {
      if (list[i].Mission.Id == missionId) {
        list[i].Responders.Loader();
        break;
      }
    }
  }
  if (typeof google != 'undefined') {
    map = new google.maps.Map(mapElement, { zoom: 10, center: new google.maps.LatLng(47.5, -122), mapTypeId: 'terrain' });
  }
}
