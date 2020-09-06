import React from 'react';
import PropTypes from 'prop-types';

import {OlProvider} from '../../context';

import olGeoJSON from 'ol/format/GeoJSON';
import OlMap from 'ol/Map';
import olView from 'ol/View';
import * as olProj from 'ol/proj';
import olZoomControl from 'ol/control/Zoom';
import olZoomSliderControl from 'ol/control/ZoomSlider';
import olScaleLineControl from 'ol/control/ScaleLine';
import olMousePositionControl from 'ol/control/MousePosition';
import olFullScreen from 'ol/control/FullScreen';
import {defaults as defaultInteractions} from 'ol/interaction.js';
import olDragPan from "ol/interaction/DragPan";
import olKinetic from "ol/Kinetic";
import * as olCoordinate from 'ol/coordinate.js';
import {boundingExtent} from 'ol/extent';
import 'ol/ol.css';

import ResizeSensor from 'css-element-queries/src/ResizeSensor'

import './style.scss';
import axios from "axios";
import featuresHelper from "../../utils/featuresHelper";
import layersHelper from "../../utils/layersHelper";
import mapHelper from "../../utils/mapHelper";
import spatialHelper from "../../utils/spatialHelper";

olZoomSliderControl.prototype.handleDraggerDrag_ = () => {
};

export default class Map extends React.Component {
  constructor(props) {
    super(props);
  }

  createOlElement(props) {
    const {...options} = props;
    const {dragPanKinetic} = props;

    options.target = this.container;

    options.controls = [
      new olMousePositionControl({
        coordinateFormat: function(coordinate) {
          return olCoordinate.format(coordinate, '{y}, {x}', 6);
        },
        projection: 'EPSG:4326',
        undefinedHTML: '',
        className: 'gis-mouse-position',
        target: 'gis-map-info-panel',
      }),
      new olScaleLineControl({
        minWidth: 60,
        units: 'metric',
        className: 'gis-scale-line-number',
        target: 'gis-map-info-panel',
      }),
      new olScaleLineControl({
        minWidth: 60,
        units: 'metric',
        className: 'gis-scale-line',
        target: 'gis-map-info-panel',
      }),
    ];

    options.layers = null;

    const viewSettings = {
      center: options.center,
      zoom: options.zoom,
    };

    if (window.gisMapState) {
      viewSettings.zoom = window.gisMapState.zoom;
      viewSettings.center = window.gisMapState.center;
    }

    Object.assign(viewSettings, options.limits);
    options.view = new olView(Object.assign(viewSettings, options.limits));

    options.interactions = defaultInteractions({
      dragPan: false,
    });

    const kinetic = new olKinetic(
      !dragPanKinetic || dragPanKinetic.decay === null || dragPanKinetic.decay === undefined ? -0.005 : dragPanKinetic.decay,
      !dragPanKinetic || dragPanKinetic.minVelocity === null || dragPanKinetic.minVelocity === undefined ? 0.05 : dragPanKinetic.minVelocity,
      !dragPanKinetic || dragPanKinetic.delay === null || dragPanKinetic.delay === undefined ? 100 : dragPanKinetic.delay,
    );

    options.interactions.push(new olDragPan({
      kinetic: kinetic
    }));

    return new OlMap(options);
  }

  get map() {
    return this.olElement;
  }

  onMapZoomChange = () => {
    if (!this.map) {
      return;
    }

    const elements = document.getElementsByClassName('gis-scale-line-number-inner');
    if (elements.length !== 0) {
      elements[0].innerText = elements[0].innerText.replace('km', 'км').replace('m', 'м');
    }

    this.zoom = this.map.getView().getZoom();
    this.onMapChanged();
  };

  onMapMoveStart = (evt) => {
    this.onMapChanged();

    if (this.map.getView().getZoom() !== this.zoom) {
      this.map.getTarget().style.cursor = '';
      return;
    }

    this.map.getTarget().style.cursor = 'grabbing';
  };

  onMapMoveEnd = (evt) => {
    this.onMapChanged();

    this.map.getTarget().style.cursor = '';
  };

  onMapClick = (event) => {
    this.onSelect(event.pointerEvent, event.pixel, event.coordinate);
  };
  onMapContextMenu = (event) => {
    event.preventDefault();

    const mapRect = document.getElementById('gis-map').getBoundingClientRect();
    const pixel = [event.x - mapRect.x, event.y - mapRect.y];
    const coordinate = this.map.getCoordinateFromPixel(pixel);
    this.onSelect(event, pixel, coordinate);
  };

  onSelect = (pointerEvent, pixel, coordinate) => {
    if (this.selectSuppressCounter > 0) {
      return;
    }

    const {layers, onSelect, gisAdapter, getAliases, clusterClickMode} = this.props;
    if (!onSelect) {
      return;
    }

    const map = this.map;
    const latlon = olProj.transform(coordinate, 'EPSG:3857', 'EPSG:4326');

    const click = {
      pointerEvent: pointerEvent,
      lat: latlon[1],
      lon: latlon[0],
    };

    const reportLayerOnMapFeature = (feature, layer) => {
      const style = feature.get('style');
      if (!style || !style.marker || style.marker.description.length === 0) {
        return;
      }
      onSelect(click, coordinate, feature, true);
      return true;
    };
    const reportLayerResult = map.forEachFeatureAtPixel(pixel, reportLayerOnMapFeature, {
      hitTolerance: 5,
      layerFilter: (layer) => layer.get('report-layer'),
    });
    if (reportLayerResult) {
      return;
    }

    const searchResultsLayerOnMapFeature = (feature, layer) => true;
    const searchResultsLayerResult = map.forEachFeatureAtPixel(pixel, searchResultsLayerOnMapFeature, {
      hitTolerance: 5,
      layerFilter: (layer) => layer.get('search-results-layer'),
    });
    if (searchResultsLayerResult) {
      return;
    }

    const promises = [];
    layersHelper.toArray(layers).filter(x => x.visible && x.type === 'layer').map((mapLayer) => {
      if (mapLayer.mapLayerType === 'WMS') {
        const viewResolution = (map.getView().getResolution());
        const params = {
          'INFO_FORMAT': 'application/json',
          'QUERY_LAYERS': mapLayer.layers,
        };

        if (mapLayer.clustered) {
          params.buffer = 100;
          params.feature_count = 1000;
        }

        const url = mapLayer.clustered ?
          mapLayer.olSource.getFeatureInfoUrl2(coordinate, viewResolution, 'EPSG:3857', params, map.getSize()) :
          mapLayer.olSource.getFeatureInfoUrl(coordinate, viewResolution, 'EPSG:3857', params);

        if (url) {
          const promise = axios.get(url).then((response) => {
            if (response.data.features) {
              response.data.features.map(x => x.mapLayer = mapLayer);
            }

            return response;
          });
          promises.push(promise)
        }
      }
    });

    Promise.all(promises).then(function(values) {
      const features = [].concat.apply([], values.filter(x => x.data.features).map(x => (x.data.features.map(feature => ({
        id: feature.id,
        mapLayer: feature.mapLayer,
        layerId: feature.mapLayer.layerId,
        geometry: (new olGeoJSON()).readFeatures(feature.geometry)[0].getGeometry(),
        geometryType: feature.geometry.type,
        properties: feature.properties,
        name: feature.properties.name,
      })))));

      // выбрать объекты из кластеризованных слоев
      const clusteredFeatures = features.filter(x => x.mapLayer.clustered);
      if (clusteredFeatures.length > 0) {
        // найти ближайшую точку
        const nearest = spatialHelper.nearestPoint(coordinate, clusteredFeatures);
        if (nearest) {
          if (nearest.properties.gs_pointstacker_envBBOX && nearest.properties.gs_pointstacker_envBBOX.length > 0 && nearest.properties.gs_pointstacker_count > 1) {
            // Env[5689930.579061592 : 1.4841451788352437E7, 5781918.037131326 : 1.485839753142183E7]
            const envBBOX = nearest.properties.gs_pointstacker_envBBOX;
            //console.log('clustered extent ' + envBBOX);
            const envBBOXcoordinates = envBBOX.substr(4).replace(']', '').split(', ');
            const coordinate1 = envBBOXcoordinates[0].split(' : ').map(x => parseFloat(x)).reverse();
            const coordinate2 = envBBOXcoordinates[1].split(' : ').map(x => parseFloat(x)).reverse();

            if (clusterClickMode !== 'none') {
              const extent = boundingExtent([coordinate1, coordinate2]);
              mapHelper.fitExtent(extent, 1000);
            }

            onSelect(click, coordinate, nearest, features, false);
            return;
          }

          onSelect(click, coordinate, nearest, features, true);
        }

        return;
      }

      const smallerFeature = featuresHelper.getSmallerFeature(features.filter(x => !x.mapLayer.clustered));
      if (smallerFeature && getAliases) {
        gisAdapter
          .getAliases(smallerFeature.layerId)
          .then((response) => {
            smallerFeature.aliases = response.data.map(x => { delete x.id; delete x.layerId; return x; });
            onSelect(click, coordinate, smallerFeature, features, true);
          });

        return;
      }

      onSelect(click, coordinate, smallerFeature, features, true);
    });
  };

  onMapChanged = () => {
    const {onExtentChanged} = this.props;
    if (onExtentChanged) {
      onExtentChanged({
        zoom: this.map.getView().getZoom(),
        extent: olProj.transformExtent(this.map.getView().calculateExtent(), 'EPSG:3857', 'EPSG:4326'),
        center: olProj.transform(this.map.getView().getCenter(), 'EPSG:3857', 'EPSG:4326'),
      });
    }
  };

  selectSuppressCounter = 0;
  selectSuppress = () => {
    this.selectSuppressCounter++;
  };
  selectUnSuppress = () => {
    this.selectSuppressCounter--;
  };
  selectToggleSuppress = (val) => {
    if (val) {
      this.selectSuppress();
    } else {
      this.selectUnSuppress();
    }
  };

  componentDidMount() {
    this.olElement = this.createOlElement(this.props);
    window.OL = this.olElement;

    if (this.props.onMapCreated) {
      this.props.onMapCreated(this.olElement);
    }

    this.olElement.selectSuppress = this.selectSuppress;
    this.olElement.selectUnSuppress = this.selectUnSuppress;
    this.olElement.selectToggleSuppress = this.selectToggleSuppress;

    this.contextValue = {
      layerContainer: this.olElement,
      map: this.olElement,
    };

    this._mapResizeHandler = new ResizeSensor(this.olElement.getTargetElement(), () => {
      this.olElement.updateSize();
    });

    this.map.on('movestart', this.onMapMoveStart);
    this.map.on('moveend', this.onMapMoveEnd);
    this.map.on('click', this.onMapClick);
    this.map.getViewport().addEventListener('contextmenu', this.onMapContextMenu);

    this.map.getView().on('change:resolution', this.onMapZoomChange);
    setTimeout(this.onMapZoomChange, 100);

    this.zoom = this.map.getView().getZoom();
    this.forceUpdate();
  }

  componentWillUnmount() {
    window.gisMapState = {
      zoom: this.map.getView().getZoom(),
      center: this.map.getView().getCenter(),
    };

    this.map.getView().un('change:resolution', this.onMapZoomChange);

    this.map.un('movestart', this.onMapMoveStart);
    this.map.un('moveend', this.onMapMoveEnd);
    this.map.un('click', this.onMapClick);
    this.map.getViewport().removeEventListener('contextmenu', this.onMapContextMenu);

    this._mapResizeHandler.reset();
    this.olElement.setTarget(undefined);
  }

  render() {
    return (
      <div
        id={'gis-map'}
        className="openlayers-map gis-map"
        ref={(el) => this.container = el}>
        {this.contextValue ? (
          <OlProvider value={this.contextValue}>
            {this.props.children}
          </OlProvider>
        ) : null}
      </div>
    )
  }
}

Map.propTypes = {
  gisAdapter: PropTypes.object,
  getAliases: PropTypes.any,
  children: PropTypes.any,
  onSelect: PropTypes.func,
  onExtentChanged: PropTypes.func,
  layers: PropTypes.oneOfType([
    PropTypes.array,
    PropTypes.bool,
  ]),
  dragPanKinetic: PropTypes.object,
  clusterClickMode:  PropTypes.string,
  onMapCreated: PropTypes.func,
};
