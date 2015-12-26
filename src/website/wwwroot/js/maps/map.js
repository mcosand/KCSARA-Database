L.kcsarStaticMap = function (map, options) {
  options = $.extend({
  }, options);

  var baseLayers = {
    "OpenStreetMap": L.tileLayer('//{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map)
  };
}

L.kcsarSetupMap = function (map, options) {
  options = $.extend({
    showLayers: true,
    showResponders: false,
    defaultBase: 'Google Streets'
  }, options);

  var baseLayers = {
    "OpenStreetMap": L.tileLayer('//{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
    })//.addTo(map)
  };


  if (typeof google !== 'undefined') {
    baseLayers["Google Satellite"] = new L.Google('HYBRID', { attribution: '' });
    baseLayers["Google Streets"] = new L.Google('ROADMAP');
  }

  if (baseLayers[options.defaultBase]) { map.addLayer(baseLayers[options.defaultBase]); }

  var geoJsonUrl = '/nwtopo/contours/{z}/{x}/{y}.json';
  var geoTileLayer = new L.TileLayer.GeoJSON(geoJsonUrl, {
    clipTiles: true,
  },
    {
      style: function (f) {
        return {
          color: '#AF591C',
          weight: (f.properties.type == 0x22) ? 2 : 1,
          opacity: 0.7,
          clickable: false
        };
      }
    });//.addTo(map);

  var trailsLayer = new L.TileLayer.GeoJSON('/nwtopo/trails/{z}/{x}/{y}.json', {
    clipTiles: true,
  },
  {
    style: function (f) {
      return (f.properties.type == 0x16) ? {
        color: '#D00',
        weight: 2.4,
        opacity: 0.8
      } : {
        color: '#222',
        weight: 1.5,
        opacity: 0.7,
        dashArray: '5, 5'
      };
    },
    onEachFeature: function (feature, layer) {
      if (feature.properties && feature.properties.name) {
        var popupString = '<div class="popup">' + String(feature.properties.name)
          .replace(/&/g, '&amp;')
          .replace(/"/g, '&quot;')
          .replace(/'/g, '&#39;')
          .replace(/</g, '&lt;')
          .replace(/>/g, '&gt;') + '</div>';
        layer.bindPopup(popupString);
      }
    }
  });//.addTo(map);


  var overlays = {
    "Trails": trailsLayer,
    "Contours (ft)": geoTileLayer
  }

  if (options.showResponders) {
    var trackLayer = L.layerGroup([]).addTo(map);
    overlays["Responders"] = trackLayer
  }

  if (options.showLayers) {
    L.control.layers(baseLayers, overlays).addTo(map);
  }
}