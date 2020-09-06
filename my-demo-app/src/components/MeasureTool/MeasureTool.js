import * as React from 'react';
import PropTypes from 'prop-types';

import * as Observable from 'ol/Observable';
import Overlay from 'ol/Overlay';
import {LineString, Polygon} from 'ol/geom';
import Draw from 'ol/interaction/Draw.js';
import VectorLayer from 'ol/layer/Vector';
import VectorSource from 'ol/source/Vector';
import olStyle from 'ol/style/Style';
import olStroke from 'ol/style/Stroke';
import olFill from 'ol/style/Fill';
import olCircle from 'ol/style/Circle';
import {getArea, getLength} from 'ol/sphere';

import IcoPolyline from '../../icons/ico_polyline.svg'
import IcoPolygon from '../../icons/ico_polygon.svg'

import './style.scss';
import spatialHelper from "../../utils/spatialHelper";

export default class MeasureTool extends React.PureComponent {
  state = {
    type: null,
  };

  constructor(props) {
    super(props);

    this.measureTooltip = null;
    this.measureTooltipElement = null;
  }

  get map() {
    return window.OL;
  }


  setTool(type) {
    if (this.state.type === type) {
      type = null;
    }

    this.setState({
      type
    }, this.initialize);
  }

  initialize() {
    const {type} = this.state;
    const map = this.map;
    let lineSegments = [];

    if (this.vectorLayer) {
      map.removeLayer(this.vectorLayer);
      this.vectorLayer = null;
    }

    this.vectorLayer = new VectorLayer({
      source: this.source,
      style: new olStyle({
        fill: new olFill({
          color: 'rgba(255, 255, 255, 0.2)',
        }),
        stroke: new olStroke({
          color: '#ffcc33',
          width: 2,
        }),
        image: new olCircle({
          radius: 7,
          fill: new olFill({
            color: '#ffcc33',
          }),
        }),
      }),
    });
    this.vectorLayer.setZIndex(9999);
    map.addLayer(this.vectorLayer);

    if (this.draw) {
      map.removeInteraction(this.draw);
      this.draw = null;
    }

    if (type === null) {
      this.map.selectUnSuppress();
      return;
    }

    this.map.selectSuppress();

    this.draw = new Draw({
      source: this.source,
      type: (type === 'Area' ? 'Polygon' : 'LineString'),
      maxPoints: Infinity,
      style: new olStyle({
        fill: new olFill({
          color: 'rgba(255, 255, 255, 0.2)',
        }),
        stroke: new olStroke({
          color: 'rgba(0, 0, 0, 0.5)',
          lineDash: [10, 10],
          width: 2,
        }),
        image: new olCircle({
          radius: 5,
          stroke: new olStroke({
            color: 'rgba(0, 0, 0, 0.7)',
          }),
          fill: new olFill({
            color: 'rgba(255, 255, 255, 0.2)',
          }),
        }),
      }),
      condition: (evt) => true,
    });
    map.addInteraction(this.draw);

    let sketch;
    let changeGeometryListener;

    const onChangeGeometry = (geom) => {
      let tooltipText;
      let tooltipCoordinate;

      if (geom instanceof Polygon) {
        tooltipText = formatArea(geom) + ', ' + formatLength(geom);
        tooltipCoordinate = geom.getInteriorPoint().getCoordinates();
      } else if (geom instanceof LineString) {
        tooltipText = formatLength(geom);
        tooltipCoordinate = geom.getLastCoordinate();

        for (let lineSegment of lineSegments) {
          this.map.removeOverlay(lineSegment);
        }
        lineSegments = [];

        const segments = spatialHelper.getLineSegments(geom);
        if (segments.length > 1) {
          for (let segment of segments) {
            const segmentCenterCoordinate = segment.properties.center.geometry.coordinates;
            const segmentLength = formatLength(new LineString(segment.geometry.coordinates));
            const segmentOverlay = this.buildMeasure(segmentCenterCoordinate, segmentLength);

            lineSegments.push(segmentOverlay);
          }
        }
      }

      this.measureTooltipElement.innerHTML = tooltipText;
      this.measureTooltip.setPosition(tooltipCoordinate);
    };

    this.draw.on('drawstart', (evt) => {
      this.fc = null;
      sketch = evt.feature;
      lineSegments = [];

      changeGeometryListener = sketch.getGeometry().on('change', (evt) => { onChangeGeometry(evt.target); });
    }, this);
    this.draw.on('drawend', (evt) => {
      this.measureTooltipElement.className = 'gis-measure-tooltip gis-measure-tooltip-static';
      this.measureTooltip.setOffset([0, -7]);

      sketch = null;
      this.measureTooltipElement = null;
      this.createMeasureTooltip();
      Observable.unByKey(changeGeometryListener);
    }, this);

    function formatLength(olFeature) {
      const length = getLength(olFeature);
      if (length > 100) {
        return (Math.round(length / 1000 * 100) / 100) + ' ' + 'км';
      }

      return (Math.round(length * 100) / 100) + ' ' + 'м';
    }
    function formatArea(olFeature) {
      const area = getArea(olFeature);
      if (area > 10000) {
        return (Math.round(area / 1000000 * 100) / 100) + ' ' + 'км<sup>2</sup>';
      }

      return (Math.round(area * 100) / 100) + ' ' + 'м<sup>2</sup>';
    }

    this.createMeasureTooltip();
  }

  clean() {
    this.setState({
      type: null,
    });

    this.map.removeInteraction(this.draw);
    this.map.removeLayer(this.vectorLayer);

    this.source.clear();
    this.draw = null;
    this.vectorLayer = null;

    const tooltips = document.getElementsByClassName('gis-measure-tooltip');
    for (let i = tooltips.length - 1; i >= 0; i--) {
      tooltips[i].parentNode.removeChild(tooltips[i]);
    }
  }

  createMeasureTooltip = () => {
    if (this.measureTooltipElement && this.measureTooltipElement.parentNode) {
      this.measureTooltipElement.parentNode.removeChild(this.measureTooltipElement);
    }

    this.measureTooltipElement = document.createElement('div');
    this.measureTooltipElement.className = 'gis-measure-tooltip gis-measure-tooltip-draw';

    this.measureTooltip = new Overlay({
      element: this.measureTooltipElement,
      offset: [0, -15],
      positioning: 'bottom-center',
      stopEvent: false,
    });
    this.map.addOverlay(this.measureTooltip);
  };

  buildMeasure = (coordinate, text) => {
    const element = document.createElement('div');
    element.className = 'gis-measure-tooltip gis-measure-tooltip-static';
    element.innerHTML = text;

    const overlay = new Overlay({
      element: element,
      offset: [0, -7],
      positioning: 'bottom-center',
      stopEvent: false,
    });
    overlay.setPosition(coordinate);

    this.map.addOverlay(overlay);

    return overlay;
  };

  onDocumentKeyDown = (e) => {
    if (e.keyCode === 27 && this.draw) {
      this.draw.setActive(false);
      this.createMeasureTooltip();

      setTimeout(() => {
        this.draw.setActive(true);
      });
    }
  };


  componentDidMount() {
    this.source = new VectorSource();

    document.addEventListener('keydown', this.onDocumentKeyDown);
  }

  componentWillUnmount() {
    this.map.removeLayer(this.olElement);

    document.removeEventListener('keydown', this.onDocumentKeyDown);
  }

  componentDidUpdate(prevProps, prevState, snapshot) {
    if (prevProps.visible !== this.props.visible) {
      if (!this.props.visible) {
        this.clean();
      }
    }
  }

  render() {
    const {visible} = this.props;
    const {type} = this.state;

    const className = visible ? 'gis-measure-panel' : 'gis-measure-panel off';

    return (
      <div className={className} ref={(el) => this.element = el}>
        <a title='Площадь и периметр' className={'gis-map-button ' + (type === 'Area' ? ' gis-map-button-active' : '')} onClick={() => this.setTool('Area') }>
          <span className={'icon'}>
            <IcoPolygon />
          </span>
        </a>
        <a title='Расстояние' className={'gis-map-button ' + (type === 'Length' ? ' gis-map-button-active' : '')} onClick={() => this.setTool('Length') }>
          <span className={'icon'}>
            <IcoPolyline />
          </span>
        </a>
      </div>
    );
  }
}

MeasureTool.propTypes = {
  visible: PropTypes.bool,
};