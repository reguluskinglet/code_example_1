import React from 'react';
import PropTypes from 'prop-types';
import './style.scss';

import olVectorSource from "ol/source/Vector";
import olDraw from "ol/interaction/Draw";
import olFill from "ol/style/Fill";
import olStroke from "ol/style/Stroke";
import olStyle from "ol/style/Style";
import olCircleStyle from "ol/style/Circle";
import olVectorLayer from "ol/layer/Vector";
import olGeoJSON from "ol/format/GeoJSON";
import olCollection from "ol/Collection";
import olModify from "ol/interaction/Modify";
import olMultiPoint from "ol/geom/MultiPoint";
import olSelect from "ol/interaction/Select";
import olTranslate from "ol/interaction/Translate";
import Icon from "ol/style/Icon";
import * as olExtent from "ol/extent";
import featuresHelper from "../../utils/featuresHelper";

import IcoPointMarker from './images/marker.png';

export default class EditorPanel extends React.PureComponent { // eslint-disable-line react/prefer-stateless-function
  state = {
    drawMode: false,
    drawType: null,
    features: []
  };

  get map() {
    return window.OL;
  }

  editorCollection = null;
  editorSource = null;
  editorLayer = null;

  modifyCollection = null;
  modifyInteraction = null;
  translateCollection = null;
  translateInteraction = null;
  drawInteraction = null;

  constructor(props) {
    super(props);

    this.editorCollection = new olCollection([], {
      unique: true,
    });

    this.modifyCollection = new olCollection([], {
      unique: true,
    });
    this.translateCollection = new olCollection([], {
      unique: true,
    });
  }

  componentDidMount() {
    const p = () => {
      if (!this.map) {
        setTimeout(p, 100);
        return;
      }

      this.map.on('singleclick', this.onMapSingleClick);
      this.map.getView().on('change:resolution', this.onMapZoomChange);
      this.onMapZoomChange();
    };
    setTimeout(p, 100);

    document.addEventListener('keydown', this.onDocumentKeyDown);
  };

  componentWillUnmount() {
    if (this.overlay) {
      this.map.removeOverlay(this.overlay);
      this.overlay = null;
    }
    if (this.select) {
      this.map.removeInteraction(this.select);
      this.select = null;
    }

    this.map.un('singleclick', this.onMapSingleClick);
    this.map.getView().un('change:resolution', this.onMapZoomChange);

    document.removeEventListener('keydown', this.onDocumentKeyDown);
  };

  componentDidUpdate(prevProps, prevState, snapshot) {
    if (prevProps.visible !== this.props.visible) {
      if (this.props.visible) {
        this.onEditStarted();
      }
    }

    if (prevProps.geoJson !== this.props.geoJson) {
      if (this.props.geoJson && this.props.geoJson.length > 0) {
        const geoJSON = new olGeoJSON();
        const geoJSONWriteOptions = {
          dataProjection: 'EPSG:4326',
          featureProjection: 'EPSG:3857',
        };

        const features = geoJSON.readFeatures(this.props.geoJson, geoJSONWriteOptions);
        this.onEditStarted(features);

        this.editorCollection.clear();
        for (let i = 0; i < features.length; i++) {
          this.onFeatureCreated(features[i]);
          this.editorCollection.push(features[i]);
        }

        if (features.length !== 0 && features[0].getGeometry().getType() === 'Point') {
          this.onSelectFeature({
            featureId: this.editorCollection.getArray()[0].getId(),
          });
        }
        else {
          this.onSelectFeature(null);
        }

        this.fit(features);
      }
    }
  }

  render() {
    return <div></div>;
  }


  onEditStarted = () => {
    if (this.state.drawMode) {
      return;
    }

    this.createdFeatureId = -1;

    this.editorCollection.clear();
    this.editorSource = new olVectorSource({
      features: this.editorCollection,
    });

    const featureStyle = (feature) => {
      let styleOptions = {};

      styleOptions.fill = new olFill({
        color: 'rgba(66, 134, 244, 0.2)',
      });
      styleOptions.stroke = new olStroke({
        color: 'rgba(66, 134, 244, 0.5)',
        width: 2,
      });

      switch (feature.getGeometry().getType()) {
        case 'Point':
        case 'MultiPoint': {
          styleOptions.image = new Icon({
            anchor: [16, 25],
            anchorXUnits: 'pixels',
            anchorYUnits: 'pixels',
            src: IcoPointMarker,
          });

          break;
        }
      }

      if (!this.state.selectedFeature || this.state.selectedFeature !== feature) {
        return new olStyle(styleOptions);
      }

      styleOptions.fill = new olFill({
        color: 'rgba(66, 134, 244, 0.2)',
      });
      styleOptions.stroke = new olStroke({
        color: 'rgba(66, 134, 244, 0.5)',
        lineDash: [10, 10],
        width: 2,
      });

      switch (feature.getGeometry().getType()) {
        case 'Polygon':
        case 'MultiPolygon':
        case 'LineString': {
          if (styleOptions.stroke) {
            styleOptions.stroke.setLineDash([10, 10]);
          }
          break;
        }
        case 'Point':
        case 'MultiPoint': {
          styleOptions.image = new Icon({
            anchor: [16, 25],
            anchorXUnits: 'pixels',
            anchorYUnits: 'pixels',
            src: IcoPointMarker,
          });

          break;
        }
      }

      styleOptions.zIndex = 99999;

      const styles = [];
      styles.push(new olStyle(styleOptions));

      const vertexStyleOptions = {
        image: new olCircleStyle({
          radius: 3,
          fill: new olFill({
            color: 'orange'
          })
        }),
        zIndex: 99999,
      };

      switch (feature.getGeometry().getType()) {
        case 'LineString': {
          vertexStyleOptions.geometry = (feature) => new olMultiPoint(feature.getGeometry().getCoordinates());
          styles.push(new olStyle(vertexStyleOptions));
          break;
        }
        case 'Polygon':
        case 'MultiPolygon': {
          vertexStyleOptions.geometry = (feature) => {
            const coordinates = feature.getGeometry().getCoordinates().reduce((acc, val) => acc.concat(val), []);
            return new olMultiPoint(coordinates);
          };
          styles.push(new olStyle(vertexStyleOptions));
          break;
        }
      }

      return styles;
    };

    this.editorLayer = new olVectorLayer({
      source: this.editorSource,
      style: featureStyle,
      zIndex: 1000,
    });
    this.map.addLayer(this.editorLayer);

    this.translateCollection.clear();
    this.translateInteraction = new olTranslate({
      features: this.translateCollection,
    });
    this.translateInteraction.on('translateend', (e) => {
      const olFeature = e.features.getArray()[0];
      this.onFeatureChanged(olFeature);
    });
    this.map.addInteraction(this.translateInteraction);

    this.modifyCollection.clear();
    this.modifyInteraction = new olModify({
      features: this.modifyCollection,
      insertVertexCondition: (e) => {
        const feature = this.editorSource.getClosestFeatureToCoordinate(e.coordinate);
        return feature === this.state.selectedFeature;
      },
      deleteCondition: (e) => {
        return e.pointerEvent.type === "pointerup" && e.pointerEvent.altKey;
      }
    });
    this.modifyInteraction.on('modifyend', (e) => {
      const olFeature = e.features.getArray()[0];
      this.onFeatureChanged(olFeature);
    });
    this.map.addInteraction(this.modifyInteraction);

    this.setState({
      drawMode: true,
    });
  };

  onEditFinished = () => {
    if (!this.state.drawMode) {
      return;
    }

    this.map.removeLayer(this.editorLayer);
    this.map.removeInteraction(this.modifyInteraction);
    this.map.removeInteraction(this.translateInteraction);
    this.map.removeInteraction(this.drawInteraction);

    this.editorCollection.clear();
    this.translateCollection.clear();
    this.modifyCollection.clear();

    this.setState({
      drawMode: false,
    });

    this.map.selectUnSuppress();
  };


  onButtonClick = (type) => {
    this.map.removeInteraction(this.drawInteraction);

    const {drawType} = this.state;
    if (drawType === type) {
      this.setState({
        drawType: null,
      });
      this.map.selectUnSuppress();
      return;
    }

    if (drawType === null) {
      this.map.selectSuppress();
    }

    this.drawInteraction = new olDraw({
      features: this.editorCollection,
      type: type,
    });
    this.drawInteraction.on('drawstart', (e) => {
      //this.onSelectFeature(null);
    });
    this.drawInteraction.on('drawend', (e) => {
      this.editorCollection.clear();

      const olFeature = e.feature;
      this.onFeatureCreated(olFeature);

      setTimeout(() => {
        this.onFeatureChanged(olFeature);

        this.map.removeInteraction(this.drawInteraction);
        for (let olFeature of this.editorCollection.getArray().filter((x) => !x.getGeometry())) {
          this.editorCollection.remove(olFeature);
        }

        this.onSelectFeature({
          featureId: olFeature.getId(),
        });

        this.setState({
          drawType: null,
        });

        this.props.onCreated();
      }, 100);

      setTimeout(() => {
        this.map.selectUnSuppress();
      }, 500);
    });

    this.map.addInteraction(this.drawInteraction);

    this.setState({
      drawType: type,
    });
  };


  onFeatureCreated = (olFeature) => {
    const featureId = this.createdFeatureId--;
    olFeature.setId(featureId);
    olFeature.set('id', featureId);
    olFeature.set('editable', true);

    this.rebuildInteractionCollections(olFeature);
  };

  onFeatureChanged = (olFeature) => {
    if (!this.props.onGeometryChanged) {
      return;
    }

    const geoJSON = new olGeoJSON();
    const geoJSONWriteOptions = {
      dataProjection: 'EPSG:4326',
      featureProjection: 'EPSG:3857',
    };

    let geoJsonFeatures = geoJSON.writeFeaturesObject(this.editorCollection.getArray(), geoJSONWriteOptions);
    for (let feature of geoJsonFeatures.features) {
      delete feature.properties;
    }

    this.props.onGeometryChanged(geoJsonFeatures);
  };

  onSelectFeature = (item) => {
    this.displayFeatures();

    if (!item) {
      this.setState({
        selectedFeature: null,
      }, () => {
        this.rebuildInteractionCollections();
        this.redrawLayer(this.editorLayer);
      });
      return;
    }

    const olFeature = this.editorSource.getFeatureById(item.featureId);
    this.rebuildInteractionCollections(olFeature);
    this.redrawLayer(this.editorLayer);
  };

  onMapZoomChange = () => {
    if (!this.map) {
      return;
    }

    this.setState({
      zoom: this.map.getView().getZoom(),
    }, () => {
      this.redrawLayer(this.editorLayer);
    });
  };
  onMapSingleClick = (evt) => {
    if (evt.pointerEvent.altKey) {
      return;
    }
    const features = this.editorCollection.getArray();
    if (features.length !== 0 && features[0].getGeometry().getType() === 'Point') {
      return;
    }

    const items = [];

    const onMapFeature = (feature, layer) => {
      const featureId = feature.getProperties().id;
      if (!featureId || items.filter(x => x.featureId === featureId).length !== 0) {
        return;
      }

      const item = {
        feature,
        featureId,
        geometry: feature.getGeometry(),
        geometryType: feature.getGeometry().getType(),
      };

      items.push(item);
    };
    this.map.forEachFeatureAtPixel(evt.pixel, onMapFeature, {
      hitTolerance: 5,
      layerFilter: (layer) => true
    });

    const smallerFeature = featuresHelper.getSmallerFeature(items);
    if (smallerFeature) {
      this.onSelectFeature(smallerFeature);
      return;
    }

    this.onSelectFeature();
  };

  displayFeatures = (features) => {
    if (this.select) {
      this.map.removeInteraction(this.select);
      this.select = null;
    }

    if (!features || features.length === 0) {
      return;
    }

    this.select = new olSelect({
      condition: function () { return false },
    });

    this.map.addInteraction(this.select);

    const selectFeatures = this.select.getFeatures();
    for (let olFeature of features) {
      try {
        selectFeatures.push(olFeature)
      } catch (exc) { }
    }
  };


  rebuildInteractionCollections = (olFeature) => {
    this.setState({
      selectedFeature: olFeature,
    });

    this.modifyCollection.clear();
    if (olFeature) {
      this.modifyCollection.push(olFeature);
    }

    this.translateCollection.clear();
    if (olFeature) {
      switch (olFeature.getGeometry().getType()) {
        case 'Polygon':
        case 'MultiPolygon': {
          this.translateCollection.push(olFeature);
          break;
        }
      }
    }
  };

  redrawLayer = (olLayer) => {
    if (olLayer) {
      olLayer.setStyle(olLayer.getStyle());
    }
  };


  fit = (features) => {
    if (!features || features.length === 0) {
      return;
    }

    let ext = features[0].getGeometry().getExtent();
    for (let i = 0; i < features.length; i++) {
      olExtent.extend(ext, features[i].getGeometry().getExtent());
    }

    if (ext[0] === ext[2] && ext[1] === ext[3]) {
      this.map.getView().setCenter([ext[0], ext[1]]);
      this.map.getView().setZoom(12);
      return;
    }

    this.map.getView().fit(ext, {
      size: this.map.getSize()
    });
  };

  center(coordinates) {
    const mapSize = this.map.getSize();
    const mapView = this.map.getView();
    mapView.centerOn(coordinates, mapSize, [mapSize[0]/2, mapSize[1]/2]);
    mapView.setZoom(mapView.getMaxZoom() - 2);
  }

  setDrawType = (type) => {
    this.onButtonClick(type);
  };
  getDrawType = () => {
    return this.state.drawType;
  };


  onRemoveActionBtnClick = () => {
    const {selectedFeature} = this.state;
    if (!selectedFeature) {
      return;
    }

    this.editorCollection.remove(selectedFeature);

    this.onSelectFeature(null);
    this.onFeatureChanged(selectedFeature)
  };

  onDocumentKeyDown = (e) => {
    if (e.keyCode === 46) {
      this.onRemoveActionBtnClick();
    }
  };
}

EditorPanel.propTypes = {
  visible: PropTypes.bool,
  layersPanelVisible: PropTypes.bool,
  geoJson: PropTypes.string,
  onGeometryChanged: PropTypes.func,
  onCreated: PropTypes.func,
};
