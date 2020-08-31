import olSourceImageWms from "ol/source/ImageWMS";
import olImage from "ol/layer/Image";
import olSourceOsm from "ol/source/OSM";
import olTile from "ol/layer/Tile";
import moment from "moment";
import {get as getProjection, transform} from "ol/proj";
import {assign} from "ol/obj";
import {DEFAULT_WMS_VERSION} from "ol/source/common";
import {calculateSourceResolution} from "ol/reproj";
import {getForViewAndSize} from "ol/extent";
import queryString from 'query-string';

export default class MapLayer {
  constructor(map, layerInfo, gisAdapter) {
    this._map = map;
    this.layerInfo = layerInfo;
    this.gisAdapter = gisAdapter;

    this.layer = MapLayer.build(layerInfo);
  }

  static build(layer) {
    if (layer.playback) {
      const source = new olSourceImageWms({
        url: layer.layerUrl + '&viewparams=' + layer.playback.param + ':' + layer.playback.currentValue.format('YYYYMMDDTHHmmss'),
        ratio: 1,
        serverType: 'geoserver',
        crossOrigin: 'anonymous',
      });

      layer.olSource = source;

      return new olImage({
        source,
        zIndex: layer.order,
        mapLayer: layer,
      });
    }

    switch (layer.mapLayerType) {
      case 'WMS': {
        //get params dynamic from url
        const params = queryString.parse(layer.layerUrl.toLocaleLowerCase());
        const layers = params.layers;
        const version = params.version;
        const styles = params.styles;

        const source = new olSourceImageWms({
          url: layer.layerUrl,
          params: {'LAYERS': layers, 'VERSION': version, 'STYLES': styles},
          ratio: 1,
          serverType: 'geoserver',
          crossOrigin: 'anonymous',
        });

        source.getFeatureInfoUrl2 = function (coordinate, resolution, projection, params, size) {
          if (this.url_ === undefined) {
            return undefined;
          }

          var projectionObj = getProjection(projection);
          var sourceProjectionObj = this.getProjection();
          if (sourceProjectionObj && sourceProjectionObj !== projectionObj) {
            resolution = calculateSourceResolution(sourceProjectionObj, projectionObj, coordinate, resolution);
            coordinate = transform(coordinate, projectionObj, sourceProjectionObj);
          }
          var extent = getForViewAndSize(coordinate, resolution, 0, size);
          var baseParams = {
            'SERVICE': 'WMS',
            'VERSION': DEFAULT_WMS_VERSION,
            'REQUEST': 'GetFeatureInfo',
            'FORMAT': 'image/png',
            'TRANSPARENT': true,
            'QUERY_LAYERS': this.params_['LAYERS']
          };
          assign(baseParams, this.params_, params);
          var x = Math.floor((coordinate[0] - extent[0]) / resolution);
          var y = Math.floor((extent[3] - coordinate[1]) / resolution);
          baseParams[this.v13_ ? 'I' : 'X'] = x;
          baseParams[this.v13_ ? 'J' : 'Y'] = y;
          return this.getRequestUrl_(extent, size, 1, sourceProjectionObj || projectionObj, baseParams);
        };

        layer.olSource = source;

        return new olImage({
          source,
          zIndex: layer.order,
          mapLayer: layer,
        });
      }
      case 'OSM': {
        const source = new olSourceOsm({
          url: layer.layerUrl,
        });
        return new olTile({
          source,
          zIndex: 0,
        });
      }
    }
  }

  get map() {
    return this._map;
  }

  addToMap() {
    if (this.layer) {
      this.map.addLayer(this.layer);
    }
    return this;
  }

  removeFromMap() {
    if (this.layer) {
      this.map.removeLayer(this.layer);
    }
    return this;
  }

  destroy() {
  }
}