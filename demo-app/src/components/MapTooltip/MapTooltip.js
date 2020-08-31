import * as React from 'react';
import PropTypes from 'prop-types';

import {withOl, OlProvider} from '../../context';

import Overlay from 'ol/Overlay';

import './style.scss';
import debounce from "lodash/debounce";
import layersHelper from "../../utils/layersHelper";
import olGeoJSON from "ol/format/GeoJSON";
import featuresHelper from "../../utils/featuresHelper";
import axios from "axios/index";

export class MapTooltip extends React.Component {
  constructor(props) {
    super(props);
    this.onMapPointerMove = debounce(this.onMapPointerMove, 200);
  }

  get layerContainer() {
    return this.props.ol.layerContainer || this.props.ol.map;
  }

  onMapPointerMove = (evt) => {
    const {layers} = this.props;

    const tooltip = this.element;
    const overlay = this.overlay;
    if (overlay.get('suppress')) {
      return;
    }

    const showTooltip = (content) => {
      tooltip.style.display = '';
      tooltip.innerHTML = content;

      const maxWidth = 310;
      const maxHeight = 310;

      let positioning = '';
      let offset = [];

      if (evt.pixel[1] + maxHeight < map.getViewport().clientHeight) {
        positioning += 'top';
        offset[1] = 0;
      } else {
        positioning += 'bottom';
        offset[1] = 0;
      }
      if (evt.pixel[0] - maxWidth > 0) {
        positioning += '-right';
        offset[0] = -10;
      } else {
        positioning += '-left';
        offset[0] = 10;
      }

      overlay.setPositioning(positioning);
      overlay.setOffset(offset);
      overlay.setPosition(evt.coordinate);
    };

    const promises = [];
    const map = this.props.ol.map;

    layersHelper.toArray(layers).filter(x => x.visible && x.type === 'layer').map((mapLayer) => {
      if (mapLayer.mapLayerType === 'WMS') {
        const viewResolution = (map.getView().getResolution());
        const params = {
          'INFO_FORMAT': 'application/json',
          'QUERY_LAYERS': mapLayer.layers,
        };
        const url = mapLayer.olSource.getFeatureInfoUrl(evt.coordinate, viewResolution, 'EPSG:3857', params);
        if (url) {
          const promise = axios.get(url);
          promise.then((x) => {
            x.mapLayer = mapLayer;
          });
          promises.push(promise)
        }
      }
    });

    const onMapFeature = (feature, layer) => {
      const style = feature.get('style');
      if (!style || !style.marker || style.marker.title.length === 0) {
        return;
      }
      showTooltip(style.marker.title);
      return true;
    };
    const result = map.forEachFeatureAtPixel(evt.pixel, onMapFeature, {
      hitTolerance: 5,
      layerFilter: (layer) => layer.get('report-layer'),
    });
    if (result) {
      return;
    }

    const searchResultsLayerOnMapFeature = (feature, layer) => true;
    const searchResultsLayerResult = map.forEachFeatureAtPixel(evt.pixel, searchResultsLayerOnMapFeature, {
      hitTolerance: 5,
      layerFilter: (layer) => layer.get('search-results-layer'),
    });
    if (searchResultsLayerResult) {
      return;
    }

    Promise.all(promises).then(function(values) {
      const features = [].concat.apply([], values.filter(x => x.data.features).map(x => (x.data.features.map(feature => ({
        id: feature.id,
        mapLayer: x.mapLayer,
        geometry: (new olGeoJSON()).readFeatures(feature.geometry)[0].getGeometry(),
        geometryType: feature.geometry.type,
        properties: feature.properties,
        name: !x.mapLayer.tooltip ? feature.properties.name : feature.properties[x.mapLayer.tooltip],
      })))));

      const smallerFeature = featuresHelper.getSmallerFeature(features);
      if (!smallerFeature || !smallerFeature.name || smallerFeature.name.length === 0 || (smallerFeature.mapLayer.clustered && smallerFeature.properties.gs_pointstacker_count > 1)) {
        tooltip.style.display = 'none';
        return false;
      }

      showTooltip(smallerFeature.name);
    });
  };

  componentDidMount() {
    const map = this.props.ol.map;
    const tooltip = this.element;

    this.overlay = new Overlay({
      element: tooltip,
      offset: [-10, 0],
      positioning: 'top-right',
      id: 'tooltip',
      className: 'gis-tooltip-overlay',
    });
    map.addOverlay(this.overlay);
    map.on('pointermove', () => {
      tooltip.style.display = 'none';
    });
    map.on('pointermove', this.onMapPointerMove);

    const onMapSingleClick = (evt) => {
      //tooltip.style.display = 'none';
    };
    map.on('singleclick', onMapSingleClick);

    map.getViewport().addEventListener('mouseout', () => {
      tooltip.style.display = 'none';
    }, false);
  }

  componentWillUnmount() {
    this.layerContainer.removeLayer(this.olElement);
  }

  render() {
    return <div className={'gis-tooltip'} ref={(el) => this.element = el}/>;
  }
}

MapTooltip.propTypes = {
  ol: PropTypes.shape({
    layerContainer: PropTypes.any,
    map: PropTypes.any,
  }),
  children: PropTypes.any,
  onBeforeShow: PropTypes.func,
  layers: PropTypes.oneOfType([
    PropTypes.array,
    PropTypes.bool,
  ]),
};

export default withOl(MapTooltip);