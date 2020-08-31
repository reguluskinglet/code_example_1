import * as React from 'react';
import PropTypes from 'prop-types';

import VectorSource from 'ol/source/Vector';
import VectorLayer from 'ol/layer/Vector';
import Point from 'ol/geom/Point';
import Style from 'ol/style/Style';
import Icon from 'ol/style/Icon';
import Feature from 'ol/Feature';
import * as olExtent from 'ol/extent';

import defaultIcon from './images/default.png';
import focusedIcon from './images/focused.png';
import selectedIcon from './images/selected.png';

import {withOl, OlProvider} from "../../../context";
import debounce from "lodash/debounce";

export class SearchResultsLayer extends React.Component {
  source = null;
  layer = null;
  defaultStyle = null;
  focusedStyle = null;
  selectedStyle = null;
  focusedId = null;
  selectedId = null;

  constructor(props) {
    super(props);
    this.onMapPointerMove = debounce(this.onMapPointerMove, 100);
  }

  componentDidUpdate = (prevProps, prevState, snapshot) => {
    if (this.layer) {
      this.layer.setVisible(true);
    }
  };

  get layerContainer() {
    return this.props.ol.layerContainer || this.props.ol.map
  }


  addFeature(id, coordinates, item) {
    if (this.source.getFeatureById(id)) {
      return;
    }

    let feature = new Feature({
      geometry: new Point(coordinates)
    });

    feature.setId(id);
    feature.setStyle(this.defaultStyle);
    feature.set('item', item);

    this.source.addFeature(feature);
  }

  removeFeature(id) {
    const feature = this.source.getFeatureById(id);
    if (feature) {
      this.source.removeFeature(feature);
    }
  }

  setFocusedFeature(id, turn) {
    if (this.selectedId && this.selectedId === id) {
      return;
    }
    if (this.focusedId && this.focusedId !== id) {
      this.setFocusedFeature(this.focusedId, false);
    }
    const feature = this.source.getFeatureById(id);
    if (feature) {
      feature.setStyle(turn ? this.focusedStyle : this.defaultStyle);
      this.focusedId = turn ? id : null;
    }
  }
  setSelectedFeature(id, turn) {
    if (this.selectedId && this.selectedId !== id) {
      this.setSelectedFeature(this.selectedId, false);
    }

    if (!id) {
      return;
    }

    const feature = this.source.getFeatureById(id);
    if (feature) {
      feature.setStyle(turn ? this.selectedStyle : this.defaultStyle);
      this.selectedId = turn ? id : null;
    }
  }

  clear() {
    this.source.clear({fast: true});
  }

  fit(coordinates) {
    if (!coordinates || coordinates.length === 0) {
      return;
    }

    const map = this.props.ol.map;
    const ext = olExtent.boundingExtent(coordinates);

    map.getView().fit(ext, {
      size: map.getSize()
    });
  }

  fitExtent(extent) {
    if (!extent || extent.length === 0) {
      return;
    }

    const map = this.props.ol.map;
    map.getView().fit(extent, {
      size: map.getSize()
    });
  }

  center(coordinates, zoom) {
    const map = this.props.ol.map;
    const mapSize = map.getSize();
    const mapView = map.getView();
    mapView.centerOn(coordinates, mapSize, [mapSize[0]/2, mapSize[1]/2]);

    if (zoom) {
      mapView.setZoom(mapView.getMaxZoom());
    }
  }

  onMapSingleClick = (evt) => {
    if (this.focusedId) {
      const feature = this.source.getFeatureById(this.focusedId);
      if (feature) {
        this.props.onSelected(feature.get('item'));
      }
      return;
    }
    if (this.selectedId) {
      const feature = this.source.getFeatureById(this.selectedId);
      if (feature) {
        this.props.onSelected(feature.get('item'));
      }
    }
  };

  onMapPointerMove = (evt) => {
    const map = this.props.ol.map;

    let featureId = null;
    map.forEachFeatureAtPixel(evt.pixel, (feature, layer) => {
      if (featureId || !feature || !layer) {
        return false;
      }

      let id;

      if (this.layer === layer) {
        id = feature.getId();
      } else {
        id = feature.getProperties().id;
      }

      if (!id) {
        return;
      }

      if (this.selectedId === id) {
        return;
      }

      featureId = id;
      return false;
    });

    if (featureId) {
      this.setFocusedFeature(featureId, true);
      this.props.onFocused(featureId);
      return;
    }

    if (this.focusedId) {
      this.setFocusedFeature(this.focusedId, false);
    }
    this.props.onFocused(null);
  };

  render() {
    const {children} = this.props;
    if (children == null) {
      return null
    }

    return this.contextValue == null ? (
      <React.Fragment>{children}</React.Fragment>
    ) : (
      <OlProvider value={this.contextValue}>{children}</OlProvider>
    )
  }

  componentDidMount() {
    this.defaultStyle = new Style({
      image: new Icon(({
        anchor: [16, 25],
        anchorXUnits: 'pixels',
        anchorYUnits: 'pixels',
        src: defaultIcon
      })),
      zIndex: 999
    });

    this.selectedStyle = new Style({
      image: new Icon(({
        anchor: [16, 25],
        anchorXUnits: 'pixels',
        anchorYUnits: 'pixels',
        src: selectedIcon
      })),
      zIndex: 9999
    });

    this.focusedStyle = new Style({
      image: new Icon(({
        anchor: [16, 25],
        anchorXUnits: 'pixels',
        anchorYUnits: 'pixels',
        src: focusedIcon
      })),
      zIndex: 99999
    });

    this.source = new VectorSource({
      features: []
    });

    this.layer = new VectorLayer({
      opacity: 1,
      source: this.source,
      zIndex: 999999,
    });
    this.layer.resultsLayer = true;
    this.layer.set('search-results-layer', true);

    this.layerContainer.addLayer(this.layer);

    const map = this.props.ol.map;

    map.on('singleclick', this.onMapSingleClick);
    map.on('pointermove', this.onMapPointerMove);
  }

  componentWillUnmount() {
    this.layerContainer.removeLayer(this.layer);
  }
}

SearchResultsLayer.propTypes = {
  ol: PropTypes.shape({
    layerContainer: PropTypes.any,
    map: PropTypes.any
  }),
  children: PropTypes.any,
  zIndex: PropTypes.number,
  visible: PropTypes.bool,
  onSelected: PropTypes.func,
  onFocused: PropTypes.func,
};

export default withOl(SearchResultsLayer);