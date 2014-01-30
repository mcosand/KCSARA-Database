/*
 * Copyright 2014 Matthew Cosand
 */
var EditResponseModel = function(myUnits){
  var self = this;
  this.Unit = ko.observable();
  this.Unit.options = ko.computed(function(){
    var myActive = [];
    var myNotActive = [];      
    $.each(myUnits, function(i,o) { (o.IsActive ? myActive : myNotActive).push(o) });      
          
    return myActive.concat(myNotActive.length > 0 ? [{Id:"disable", Name:"--My Other Units--"}] : [], myNotActive)
  }, this);
    
  this.Location = ko.computed({
    read: function () {
      ret = { Type: self.Location.Type() };
      if (ret.Type == 'geo') {
        ret['Coords'] = self.Location.Coords();
      }
      return ret;
    }, deferEvaluation: true
  });
  this.Location.Type = ko.observable('none');
  this.Location.GeoReason = ko.observable(' - Looking');
  this.Location.Coords = ko.observable(null);

  locationUpdate = function (geo) {
    console.log(geo);
    if (geo.coords.accuracy > 1000) {
      self.Location.GeoReason(' - Not Accurate');
    }
    else if (self.Location.GeoReason() != null) {
      self.Location.GeoReason(null);
      self.Location.Type('geo');
    }
    self.Location.Coords(geo.coords);
  }

  locationErr = function (err) {
    self.Location.GeoReason(
      err.code == err.POSITION_UNAVAILABLE ? ' - Unavailable' :
      err.code == err.PERMISSION_DENIED ? ' - Denied' :
      ' - Unknown error');
    console.log(err);
  }

  if (navigator.geolocation) { self.Location.watchId = navigator.geolocation.watchPosition(locationUpdate, locationErr, {enableHighAccuracy:true}); } else { self.Location.Reason(' - Unavailable');}
};

var PageModel = function (missionInfo, myUnits) {
  var self = this;
  this.Info = ko.observable({});
  this.MyUnits = myUnits;
  this.MyUnitLookup = {};
  for (var i = 0, len = myUnits.length; i < len; i++) {
    this.MyUnitLookup[myUnits[i].Id] = myUnits[i];
    myUnits[i].IsActive = false;
  }

  this.SetMission = function (missionInfo) {
    mapMoment(missionInfo.Mission, ['StartTime']);
    mapMoment(missionInfo, ['NextStart', 'StopStaging']);
    for (entry in self.MyUnitLookup) entry.IsActive = false;

    $.each(missionInfo.ActiveUnits, function(i,u) {
      myUnit = self.MyUnitLookup[u.UnitId];
      u.AmMember = (myUnit != undefined);
      if (myUnit !== undefined) myUnit.IsActive = true;
    });
    self.Info(missionInfo);
  }
  this.SetMission(missionInfo);

  this.MyUnitResponding = ko.computed(function() {
    return $.grep(this.Info().ActiveUnits, function(o) { return o.AmMember }).length > 0;
    //return false;
  }, this);

  this.EditResponse = ko.observable(null);
  this.startEdit = function(){
    self.EditResponse(new EditResponseModel(self.MyUnits, self.Info().ActiveUnits));
  }

  this.postResponse = function()
  {
  }
  this.IsWorking = ko.observable(false);
};

