import React from 'react';
import PropTypes from 'prop-types';
import './style.scss';

import olVectorSource from "ol/source/Vector";
import olDraw from "ol/interaction/Draw";
import {createBox} from 'ol/interaction/Draw';
import * as olExtent from "ol/extent";
import olFill from "ol/style/Fill";
import olStroke from "ol/style/Stroke";
import olStyle from "ol/style/Style";
import olText from "ol/style/Text";
import olIcon from "ol/style/Icon";
import olCircleStyle from "ol/style/Circle";
import olVectorLayer from "ol/layer/Vector";
import olGeoJSON from "ol/format/GeoJSON";
import olCollection from "ol/Collection";
import olModify from "ol/interaction/Modify";
import olMultiPoint from "ol/geom/MultiPoint";
import olLineString from "ol/geom/LineString";
import olPolygon from "ol/geom/Polygon";
import {fromCircle} from "ol/geom/Polygon";
import olCircle from "ol/geom/Circle";
import olSelect from "ol/interaction/Select";
import olSnap from "ol/interaction/Snap";
import olTranslate from "ol/interaction/Translate";
import lodash from 'lodash'
import { SketchPicker } from 'react-color'
import Select from 'react-select'
import Switch from "react-switch";
import canvg from 'canvg'
import colorParse from 'color-parse'
import Konva from 'konva';
import featuresHelper from "../../utils/featuresHelper";

import IcoLine from '../../icons/ico_line.svg'
import IcoPolyline from '../../icons/ico_polyline.svg'
import IcoPolygon from '../../icons/ico_polygon.svg'
import IcoEllipse from '../../icons/ico_ellipse.svg'
import IcoCircle from '../../icons/ico_circle.svg'
import IcoRectangle from '../../icons/ico_rectangle.svg'
import IcoPointMarker from '../../icons/ico_point marker.svg'
import IcoArrow from '../../icons/ico_arrow.svg'
import IcoText from '../../icons/ico_Text.svg'
import IcoArrowLeft from '../../icons/ico_arrow_left.svg'
import IcoArrowRight from '../../icons/ico_arrow_right.svg'
import IcoMarkerSmallSize from '../../icons/ico_marker_s_size.svg'
import IcoMarkerMediumSize from '../../icons/ico_marker_m_size.svg'
import IcoMarkerLargeSize from '../../icons/ico_marker_l_size.svg'
import IcoMarkerPercentLink from '../../icons/ico_marker_percent_link.svg'
import IcoMarkerCircle from '../../icons/ico_marker_circle.svg'
import IcoMarkerDot from '../../icons/ico_marker_dot.svg'
import IcoMarkerNone from '../../icons/ico_marker_none.svg'
import IcoMarkerPin from '../../icons/ico_marker_pin.svg'
import IcoMarkerSquare from '../../icons/ico_marker_square.svg'

import IcoLayerDown from '../../icons/ico_layer_down.svg';
import IcoLayerUp from '../../icons/ico_layer_up.svg';
import IcoLayerToBottom from '../../icons/ico_layer_to_bottom.svg';
import IcoLayerToFront from '../../icons/ico_layer_to_front.svg'

import RBush from 'ol/structs/RBush';
import _ol_extent_ from "ol/extent";

// issue OL6310
const RBushUpdateOrig = RBush.prototype.update;
RBush.prototype.update = function () {
  try {
    RBushUpdateOrig.apply(this, arguments);
  } catch (e) {
    console.warn('RBushUpdateOrig', e);
  }
};

export default class ReportPanel extends React.PureComponent { // eslint-disable-line react/prefer-stateless-function
  state = {
    drawMode: false,
    drawType: null,
    features: [],

    selectedFeature: null,
    style: {},

    strokeColorPicker: false,
    strokeExtraColorPicker: false,
    extraStrokeColorPicker: false,
    fillColorPicker: false,
    textColorPicker: false,
    markerColorPicker: false,
    markerContentColorPicker: false,
  };

  get map() {
    return window.OL;
  }

  markers = null;

  editorCollection = null;
  editorSource = null;
  editorLayer = null;

  modifyCollection = null;
  modifyInteraction = null;
  translateCollection = null;
  translateInteraction = null;
  snapFeatures = null;
  snapCollection = null;
  snapInteraction = null;
  drawInteraction = null;

  constructor(props) {
    super(props);

    this.strokeWidthOptions = [];
    for (let i = 1; i <= 10;) {
      this.strokeWidthOptions.push({ value: i, label: i + ' px' });
      i = i + 0.5;
    }

    this.extraStrokeWidthOptions = [];
    for (let i = 1; i <= 10;) {
      this.extraStrokeWidthOptions.push({ value: i, label: i + ' px' });
      i = i + 0.5;
    }

    this.textSizeOptions = [];
    for (let i = 1; i <= 72; i++) {
      this.textSizeOptions.push({ value: i, label: i + ' px' });
    }

    this.markerSizeOptions = [];
    [12, 14, 16, 18, 20, 22, 24, 26, 30, 32, 36, 40, 44, 48, 52, 56, 60, 64, 72].map(i => this.markerSizeOptions.push({ value: i, label: i + ' px' }));

    this.switchProps = {
      height: 12,
      width: 32,
      handleDiameter: 20,
      uncheckedIcon: false,
      checkedIcon: false,
      boxShadow: "0px 1px 5px rgba(0, 0, 0, 0.6)",
      activeBoxShadow: "0px 0px 1px 10px rgba(0, 0, 0, 0.2)",
    };

    this.editorCollection = new olCollection([], {
      unique: true,
    });

    this.modifyCollection = new olCollection([], {
      unique: true,
    });
    this.translateCollection = new olCollection([], {
      unique: true,
    });
    this.snapCollection = new olCollection([], {
      unique: true,
    });

    this.snapFeatures = {};


    this.editorSource = new olVectorSource({
      features: this.editorCollection,
    });

    this.editorLayer = new olVectorLayer({
      source: this.editorSource,
      style: this.buildFeatureStyle,
      zIndex: 1000,
    });
    this.editorLayer.set('report-layer', true);
  }

  componentDidMount() {
    const p = () => {
      if (!this.map) {
        setTimeout(p, 100);
        return;
      }

      this.map.on('singleclick', this.onMapSingleClick);
      this.map.on('moveend', this.onMapMoveEnd);

      this.map.getView().on('change:resolution', this.onMapZoomChange);
      this.onMapZoomChange();
    };
    setTimeout(p, 100);

    document.addEventListener('keydown', this.onDocumentKeyDown);
    document.addEventListener('keyup', this.onDocumentKeyUp);

    this.markers = [];
    this.props.gisAdapter.getMarkers().then((result) => {
      this.markers = result.data;
    });
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
    this.map.un('moveend', this.onMapMoveEnd);
    this.map.getView().un('change:resolution', this.onMapZoomChange);

    document.removeEventListener('keydown', this.onDocumentKeyDown);
    document.removeEventListener('keyup', this.onDocumentKeyUp);
  };

  componentDidUpdate(prevProps, prevState, snapshot) {
    if (prevProps.visible !== this.props.visible) {
      if (this.props.visible) {
        this.onEditStarted();
      } else {
        this.onEditFinished();
      }
    }

    if (prevProps.reportJson !== this.props.reportJson) {
      if (this.props.reportJson && this.props.reportJson.length > 0) {
        const geoJSON = new olGeoJSON();
        const geoJSONWriteOptions = {
          //dataProjection: 'EPSG:4326',
          //featureProjection: 'EPSG:900913',
        };

        const json = JSON.parse(this.props.reportJson);
        const features = geoJSON.readFeatures(json.features, geoJSONWriteOptions);

        this.onEditStarted();
        this.onDeselectFeatures();

        this.editorCollection.clear();
        for (let i = 0; i < features.length; i++) {
          this.onFeatureCreated(features[i]);
          this.editorCollection.push(features[i]);
        }

        this.map.getView().setZoom(json.zoom);

        const mapSize = this.map.getSize();
        this.map.getView().centerOn(json.center, mapSize, [mapSize[0]/2, mapSize[1]/2]);
      }
    }
  }


  onChangeLayers = (layers) => {
    if (this.editorLayer) {
      const layer = layers.find(x => x.path === 'reportLayer');
      this.editorLayer.setVisible(layer.visible);
    }
  };


  render() {
    const {drawType, selectedFeature} = this.state;
    const {visible, layersPanelVisible} = this.props;
    const className = visible ? 'gis-report-panel' : 'gis-report-panel off';

    return (
      <div className={className+ (layersPanelVisible ? ' visible-layers-panel' : '')}>
        <div className={'types'}>
          <a className={'gis-line-button' + (drawType === 'Line' ? ' selected' : '')} onClick={() => {this.onButtonClick('Line')}}>
            <span className={'icon'}>
              <IcoLine />
            </span>
          </a>
          <a className={'gis-polyline-button' + (drawType === 'Polyline' ? ' selected' : '')} onClick={() => {this.onButtonClick('Polyline')}}>
            <span className={'icon'}>
              <IcoPolyline />
            </span>
          </a>
          <a className={'gis-polygon-button' + (drawType === 'Polygon' ? ' selected' : '')} onClick={() => {this.onButtonClick('Polygon')}}>
            <span className={'icon'}>
              <IcoPolygon />
            </span>
          </a>
          <a className={'gis-rectancle-button' + (drawType === 'Rectangle' ? ' selected' : '')} onClick={() => {this.onButtonClick('Rectangle')}}>
            <span className={'icon'}>
              <IcoRectangle width={32} height={32}  />
            </span>
          </a>
          <a className={'gis-ellipse-button' + (drawType === 'Ellipse' ? ' selected' : '')} onClick={() => {this.onButtonClick('Ellipse')}}>
            <span className={'icon'}>
              <IcoEllipse />
            </span>
          </a>
          <a className={'gis-circle-button' + (drawType === 'Circle' ? ' selected' : '')} onClick={() => {this.onButtonClick('Circle')}}>
            <span className={'icon'}>
              <IcoCircle width={32} height={32} />
            </span>
          </a>
          <a className={'gis-marker-button' + (drawType === 'Marker' ? ' selected' : '')} onClick={() => {this.onButtonClick('Marker')}}>
            <span className={'icon'}>
              <IcoPointMarker />
            </span>
          </a>
          <a className={'gis-arrow-button' + (drawType === 'Arrow' ? ' selected' : '')} onClick={() => {this.onButtonClick('Arrow')}}>
            <span className={'icon'}>
              <IcoArrow />
            </span>
          </a>
          <a className={'gis-text-button' + (drawType === 'Text' ? ' selected' : '')} onClick={() => {this.onButtonClick('Text')}}>
            <span className={'icon'}>
              <IcoText />
            </span>
          </a>
        </div>
        {!selectedFeature ? null : <div className={'settings'}>
          <div className={'header'}>Параметры</div>
          <div className={'content gis-scrollbar'}>
            {this.renderParams()}
          </div>
        </div>}
      </div>
    );
  }

  renderParams() {
    const {selectedFeature} = this.state;
    if (!selectedFeature) {
      return <div></div>;
    }

    switch (selectedFeature.get('type')) {
      case 'Line': {
        return <div>
          {this.renderOrderParams()}
          {this.renderLineParams()}
        </div>;
      }
      case 'Polyline': {
        return <div>
          {this.renderOrderParams()}
          {this.renderPolylineParams()}
        </div>;
      }
      case 'Polygon': {
        return <div>
          {this.renderOrderParams()}
          {this.renderPolygonParams()}
        </div>;
      }
      case 'Ellipse': {
        return <div>
          {this.renderOrderParams()}
          {this.renderEllipseParams()}
        </div>;
      }
      case 'Circle': {
        return <div>
          {this.renderOrderParams()}
          {this.renderCircleParams()}
        </div>;
      }
      case 'Rectangle': {
        return <div>
          {this.renderOrderParams()}
          {this.renderRectangleParams()}
        </div>;
      }
      case 'Arrow': {
        return <div>
          {this.renderOrderParams()}
          {this.renderArrowParams()}
        </div>;
      }
      case 'Text': {
        return <div>
          {this.renderOrderParams()}
          {this.renderTextParams()}
        </div>;
      }
      case 'Marker': {
        return <div>
          {this.renderOrderParams()}
          {this.renderMarkerParams()}
        </div>;
      }
    }

    return <div/>;
  };

  renderLineParams() {
    const {style} = this.state;

    return <div>
      {this.renderStrokeParams()}
      {this.renderExtraStrokeParams()}
    </div>;
  }

  renderPolylineParams() {
    const {style} = this.state;

    return <div>
      {this.renderStrokeParams()}
      {this.renderExtraStrokeParams()}
    </div>;
  }

  renderPolygonParams() {
    const {style} = this.state;

    return <div>
      {this.renderFillParams()}
      {this.renderStrokeParams()}
      {this.renderExtraStrokeParams()}
    </div>;
  }

  renderEllipseParams() {
    const {style} = this.state;

    return <div>
      {this.renderFillParams()}
      {this.renderStrokeParams()}
      {this.renderExtraStrokeParams()}
    </div>;
  }

  renderCircleParams() {
    const {style} = this.state;

    return <div>
      {this.renderFillParams()}
      {this.renderStrokeParams()}
      {this.renderExtraStrokeParams()}
    </div>;
  }

  renderRectangleParams() {
    const {style} = this.state;

    return <div>
      {this.renderFillParams()}
      {this.renderStrokeParams()}
      {this.renderExtraStrokeParams()}
    </div>;
  }

  renderArrowParams() {
    const {style} = this.state;

    const arrowSizeOptions = [
      { value: 0.5, label: '0.5' },
      { value: 1, label: '1' },
      { value: 2, label: '2' },
    ];

    return <div>
      {this.renderStrokeParams()}
      <div className={'param'}>
        <div className={'param-name'}>Стрелочки</div>
        <div className={'param-content'}>
          <div className={'btn first begin-arrow' + (style.arrow.begin ? ' active' : '')} onClick={this.onChangeArrowBegin}>
            <IcoArrowLeft />
          </div>
          <div className={'btn last end-arrow' + (style.arrow.end ? ' active' : '')} onClick={this.onChangeArrowEnd}>
            <IcoArrowRight />
          </div>
        </div>
      </div>
      <div className={'param'}>
        <div className={'param-name'}>Размер стрелочек</div>
        <div className={'param-content'}>
          <Select
            value={arrowSizeOptions.find((x) => x.value === style.arrow.beginSize)}
            options={arrowSizeOptions}
            className={'arrow-begin-size'}
            classNamePrefix={'map-report'}
            onChange={this.onChangeArrowBeginSize}
          />
          <div className={'btn arrow-equals-size' + (style.arrow.equalsSize ? ' active' : '')} title={''} onClick={this.onArrowEqualsSize}>
            <IcoMarkerPercentLink />
          </div>
          <Select
            value={arrowSizeOptions.find((x) => x.value === style.arrow.endSize)}
            options={arrowSizeOptions}
            className={'arrow-end-size'}
            classNamePrefix={'map-report'}
            onChange={this.onChangeArrowEndSize}
          />
          <div className={'clear'}></div>
        </div>
      </div>
      {this.renderExtraStrokeParams()}
    </div>;
  }
  onChangeArrowBegin = (option) => {
    this.updateFeatureStyle({
      arrow: {
        begin: !this.state.style.arrow.begin,
      },
    });
  };
  onChangeArrowEnd = (option) => {
    this.updateFeatureStyle({
      arrow: {
        end: !this.state.style.arrow.end,
      },
    });
  };
  onChangeArrowBeginSize = (option) => {
    const {style} = this.state;

    this.updateFeatureStyle({
      arrow: {
        beginSize: option.value,
        endSize: style.arrow.equalsSize ? option.value : style.arrow.endSize,
      },
    });
  };
  onChangeArrowEndSize = (option) => {
    const {style} = this.state;

    this.updateFeatureStyle({
      arrow: {
        endSize: option.value,
        beginSize: style.arrow.equalsSize ? option.value : style.arrow.beginSize,
      },
    });
  };
  onArrowEqualsSize = () => {
    const {style} = this.state;

    this.updateFeatureStyle({
      arrow: {
        equalsSize: !style.arrow.equalsSize,
        endSize: style.arrow.equalsSize ? style.arrow.endSize : style.arrow.beginSize,
      },
    });
  };

  renderTextParams() {
    const {style} = this.state;

    const fontOptions = [
      { value: 'Arial', label: 'Arial' },
      { value: 'Verdana', label: 'Verdana' },
      { value: 'Times New Roman', label: 'Times New Roman' },
      { value: 'Courier New', label: 'Courier New' },
      { value: 'serif', label: 'serif' },
      { value: 'sans-serif', label: 'sans-serif' },
    ];

    return <div>
      <div className={'section'}>
        <div className={'param'}>
          <div className={'param-name'}>Текст</div>
          <textarea rows='3' value={style.text.value} placeholder='' onChange={this.onChangeTextValue} onKeyDown={this.onTextKeyDown}/>
        </div>
        <div className={'param'}>
          <div className={'param-name'}>Шрифт</div>
          <Select value={fontOptions.find((x) => x.value === style.text.font)} options={fontOptions} classNamePrefix={'map-report'} onChange={this.onChangeTextFont}/>
        </div>
        <div className={'param'}>
          <div className={'param-name'}>Размер</div>
          <Select value={this.textSizeOptions.find((x) => x.value === style.text.size)} options={this.textSizeOptions} classNamePrefix={'map-report'} onChange={this.onChangeTextSize}/>
        </div>
        <div className={'param'}>
          <div className={'param-name'}>Цвет</div>
          <div className={'param-color'}>
            <input type='text' value={style.text.color} placeholder='' onChange={this.onChangeTextColor} onKeyDown={this.onTextKeyDown}/>
            <div className={'param-color-btn'} style={{backgroundColor: style.text.color}} onClick={() => { this.setState({textColorPicker: !this.state.textColorPicker}) }}></div>
          </div>
          {this.state.textColorPicker ? <SketchPicker color={style.text.color} onChange={ this.onPickerChangeTextColor } /> : <div></div>}
        </div>

        <div className={'param'}>
          <div className={'param-name'}>Стиль</div>
          <div className={'btn first bold' + (style.text.bold ? ' active' : '')} onClick={this.onChangeTextBold}>Ж</div>
          <div className={'btn last italic' + (style.text.italic ? ' active' : '')} onClick={this.onChangeTextItalic}>К</div>
        </div>
        {this.renderExtraStrokeParams()}
      </div>
    </div>;
  }
  onChangeTextValue = (event) => {
    this.updateFeatureStyle({
      text: {
        value: event.target.value,
      },
    });
  };
  onChangeTextFont = (option) => {
    this.updateFeatureStyle({
      text: {
        font: option.value,
      },
    });
  };
  onChangeTextSize = (option) => {
    this.updateFeatureStyle({
      text: {
        size: option.value,
      },
    });
  };
  onChangeTextColor = (event) => {
    this.updateFeatureStyle({
      text: {
        color: event.target.value,
      },
    });
  };
  onPickerChangeTextColor = (color) => {
    this.updateFeatureStyle({
      text: {
        color: 'rgba(' + color.rgb.r + ', ' + color.rgb.g + ', ' + color.rgb.b + ', ' + color.rgb.a + ')',
      },
    });
  };
  onChangeTextBold = (event) => {
    this.updateFeatureStyle({
      text: {
        bold: !this.state.style.text.bold,
      },
    });
  };
  onChangeTextItalic = (event) => {
    this.updateFeatureStyle({
      text: {
        italic: !this.state.style.text.italic,
      },
    });
  };

  renderMarkerParams() {
    const {style} = this.state;

    const customSingleValue = ({ data }) => (
      <div className="input-select">
        <div className="input-select__single-value">
          { data.icon && <span className="input-select__icon"><img src={data.icon}/></span> }
          <span>{ data.label }</span>
        </div>
      </div>
    );
    const formatOptionLabel = (data) => (
      <div>
        <span className={'input-select__icon' + (data.value === style.marker.id ? ' selected' : '')}><img src={data.icon}/></span>
        <span>{data.label}</span>
      </div>
    );

    const markerImageOptions = this.markers.map(x => ({
      value: x.id,
      label: x.name,
      icon: this.props.gisAdapter.getMarkerUrl(x.link),
    }));

    const marker = this.markers.find(x => x.id === style.marker.id);

    return <div>
      <div className={'section'}>
        <div className={'param'}>
          <div className={'param-name'}>Подпись метки</div>
          <input type="text" value={style.marker.title} placeholder='' onChange={this.onChangeMarkerTitle} onKeyDown={this.onTextKeyDown}/>
        </div>
        <div className={'param'}>
          <div className={'param-name'}>Описание</div>
          <textarea rows='5' value={style.marker.description} placeholder='' onChange={this.onChangeMarkerDescription} onKeyDown={this.onTextKeyDown}/>
        </div>
        <div className={'param'}>
          <div className={'param-name'}>Вид маркера</div>
          <div className={'btn first view-none' + (style.marker.view === 'none' ? ' active' : '')} title={'без типа'} onClick={() => this.onChangeMarkerView('none')}>
            <IcoMarkerNone />
          </div>
          <div className={'btn view-point' + (style.marker.view === 'point' ? ' active' : '')} title={'точка'} onClick={() => this.onChangeMarkerView('point')}>
            <IcoMarkerDot />
          </div>
          <div className={'btn view-pin' + (style.marker.view === 'pin' ? ' active' : '')} title={'пин (капля)'} onClick={() => this.onChangeMarkerView('pin')}>
            <IcoMarkerPin />
          </div>
          <div className={'btn view-circle' + (style.marker.view === 'circle' ? ' active' : '')} title={'окружность'} onClick={() => this.onChangeMarkerView('circle')}>
            <IcoMarkerCircle />
          </div>
          <div className={'btn last view-square' + (style.marker.view === 'square' ? ' active' : '')} title={'квадрат'} onClick={() => this.onChangeMarkerView('square')}>
            <IcoMarkerSquare />
          </div>
        </div>
        <div className={'param'}>
          <div className={'param-name'}>Размер</div>
          <Select value={this.markerSizeOptions.find((x) => x.value === style.marker.size)} options={this.markerSizeOptions} classNamePrefix={'map-report'} onChange={this.onChangeMarkerSize}/>
        </div>
        {
          style.marker.view !== 'none' ?
            <div className={'param'}>
              <div className={'param-name'}>Цвет</div>
              <div className={'param-color'}>
                <input type='text' value={style.marker.color} placeholder='' onChange={this.onChangeMarkerColor} onKeyDown={this.onTextKeyDown}/>
                <div className={'param-color-btn'} style={{backgroundColor: style.marker.color}} onClick={() => { this.setState({markerColorPicker: !this.state.markerColorPicker}) }}></div>
              </div>
              {this.state.markerColorPicker ? <SketchPicker width={200} color={style.marker.color} onChange={ this.onPickerChangeMarkerColor } /> : <div></div>}
            </div> : null
        }
        {
          (style.marker.content === 'number' || style.marker.content === 'icon') && style.marker.view !== 'none' && style.marker.view !== 'point' ?
            <div className={'param switch'}>
              <Switch {...this.switchProps} onChange={this.onChangeMarkerFill} checked={style.marker.fill} className={'react-switch ' + (style.marker.fill ? 'active' : '')}/>
              <span className={'param-name'}>Сплошная заливка</span>
            </div> : null
        }
        {
          style.marker.view !== 'point' ?
            <div className={'param'}>
              <div className={'param-name'}>Состав</div>
              <div className={'btn first content-none' + (style.marker.content === 'none' ? ' active' : '')} title={'нет'} onClick={() => this.onChangeMarkerContent('none')}>нет</div>
              <div className={'btn content-number' + (style.marker.content === 'number' ? ' active' : '')} title={'число'} onClick={() => this.onChangeMarkerContent('number')}>число</div>
              <div className={'btn last content-icon' + (style.marker.content === 'icon' ? ' active' : '')} title={'иконка'} onClick={() => this.onChangeMarkerContent('icon')}>иконка</div>
            </div> : null
        }
        {
          style.marker.content === 'number' && style.marker.view !== 'point' ?
            <div className={'param'}>
              <input type="text" value={style.marker.number} placeholder='' onChange={this.onChangeMarkerNumber} onKeyDown={this.onTextKeyDown}/>
            </div> : null
        }
        {
          style.marker.content === 'icon' && style.marker.view !== 'point' ?
            <div className={'param'}>
              <Select menuPlacement="top" components={ {SingleValue: customSingleValue} } formatOptionLabel={formatOptionLabel} value={markerImageOptions.find((x) => x.value === style.marker.id)} options={markerImageOptions} onChange={this.onChangeMarkerImage}/>
            </div> : null
        }
        {
          ((style.marker.content === 'number') || (style.marker.content === 'icon' && marker.markerType === 'VECTOR')) && style.marker.view !== 'point' ?
            <div>
              <div className={'param'}>
                <div className={'param-name'}>Цвет {style.marker.content === 'number' ? 'текста' : 'иконки'}</div>
                <div className={'param-color'}>
                  <input type='text' value={style.marker.contentColor} placeholder='' onChange={this.onChangeMarkerContentColor} onKeyDown={this.onTextKeyDown}/>
                  <div className={'param-color-btn'} style={{backgroundColor: style.marker.contentColor}} onClick={() => { this.setState({markerContentColorPicker: !this.state.markerContentColorPicker}) }}></div>
                </div>
                {this.state.markerContentColorPicker ? <SketchPicker width={200} color={style.marker.contentColor} onChange={ this.onPickerChangeMarkerContentColor } /> : <div></div>}
              </div>
            </div> : null
        }
      </div>
    </div>;
  }
  onChangeMarkerTitle = (event) => {
    this.updateFeatureStyle({
      marker: {
        title: event.target.value,
      },
    });
  };
  onChangeMarkerDescription = (event) => {
    this.updateFeatureStyle({
      marker: {
        description: event.target.value,
      },
    });
  };
  onChangeMarkerView = (view) => {
    this.updateFeatureStyle({
      marker: {
        view: view,
      },
    });
  };
  onChangeMarkerContent = (content) => {
    this.updateFeatureStyle({
      marker: {
        content: content,
      },
    });
  };
  onChangeMarkerSize = (option) => {
    this.updateFeatureStyle({
      marker: {
        size: option.value,
      },
    });
  };
  onChangeMarkerNumber = (event) => {
    this.updateFeatureStyle({
      marker: {
        number: event.target.value,
      },
    });
  };
  onChangeMarkerImage = (option) => {
    this.updateFeatureStyle({
      marker: {
        id: option.value,
      },
    });
  };
  onChangeMarkerColor = (event) => {
    this.updateFeatureStyle({
      marker: {
        color: event.target.value,
      },
    });
  };
  onChangeMarkerContentColor = (event) => {
    this.updateFeatureStyle({
      marker: {
        contentColor: event.target.value,
      },
    });
  };
  onChangeMarkerFill = (checked, e, i) => {
    this.updateFeatureStyle({
      marker: {
        fill: checked,
      },
    });
  };
  onPickerChangeMarkerColor = (color) => {
    this.updateFeatureStyle({
      marker: {
        color: 'rgba(' + color.rgb.r + ', ' + color.rgb.g + ', ' + color.rgb.b + ', ' + color.rgb.a + ')',
      },
    });
  };
  onPickerChangeMarkerContentColor = (color) => {
    this.updateFeatureStyle({
      marker: {
        contentColor: 'rgba(' + color.rgb.r + ', ' + color.rgb.g + ', ' + color.rgb.b + ', ' + color.rgb.a + ')',
      },
    });
  };

  renderStrokeParams(sectionName) {
    const {style} = this.state;

    const typeOptions = [
      { value: 'solid', label: 'Сплошная' },
      { value: 'dotted', label: 'Пунктирная' },
      { value: 'dot-and-dash', label: 'Штрихпунктирная' },
    ];

    return <div>
      <div className={'section'}>
        {sectionName ? <div className={'section-name'}>{sectionName}</div> : null}
        {sectionName ? <div className={'section-name-line'}></div> : null}
        {sectionName ? <div className={'section-name-space'}></div> : null}
        <div className={'param'}>
          <div className={'param-name'}>Толщина линии</div>
          <Select value={this.strokeWidthOptions.find((x) => x.value === style.stroke.width)} options={this.strokeWidthOptions} classNamePrefix={'map-report'} onChange={this.onChangeStrokeWidth}/>
        </div>
        <div className={'param'}>
          <div className={'param-name'}>Цвет линии</div>
          <div className={'param-color'}>
            <input type='text' value={style.stroke.color} placeholder='' onChange={this.onChangeStrokeColor} onKeyDown={this.onTextKeyDown}/>
            <div className={'param-color-btn'} style={{backgroundColor: style.stroke.color}} onClick={() => { this.setState({strokeColorPicker: !this.state.strokeColorPicker}) }}></div>
          </div>
          {this.state.strokeColorPicker ? <SketchPicker color={style.stroke.color} onChange={ this.onPickerChangeStrokeColor } /> : <div></div>}
        </div>
        <div className={'param'}>
          <div className={'param-name'}>Тип линии</div>
          <Select value={typeOptions.find((x) => x.value === style.stroke.type)} options={typeOptions} classNamePrefix={'map-report'} onChange={this.onChangeStrokeType}/>
        </div>
      </div>
    </div>;
  }
  onPickerChangeStrokeColor = (color) => {
    this.updateFeatureStyle({
      stroke: {
        color: 'rgba(' + color.rgb.r + ', ' + color.rgb.g + ', ' + color.rgb.b + ', ' + color.rgb.a + ')',
      },
    });
  };
  onChangeStrokeColor = (event) => {
    this.setState({
      strokeColorPicker: false,
    });

    this.updateFeatureStyle({
      stroke: {
        color: event.target.value,
      },
    });
  };
  onChangeStrokeWidth = (option) => {
    this.updateFeatureStyle({
      stroke: {
        width: option.value,
      },
    });
  };
  onChangeStrokeType = (option) => {
    this.updateFeatureStyle({
      stroke: {
        type: option.value,
      },
    });
  };

  renderExtraStrokeParams() {
    const {style} = this.state;

    return <div className={'section extra-stroke'}>
      <div className={'param'}>
        <Switch {...this.switchProps} onChange={this.onChangeExtraStrokeEnabled} checked={style.extraStroke.enabled} className={'react-switch ' + (style.extraStroke.enabled ? 'active' : '')}/>
        <span className={'param-name'}>Внешняя обводка</span>
      </div>
      {
        style.extraStroke.enabled ? <div className={'param'}>
          <div className={'param-name'}>Цвет обводки</div>
          <div className={'param-color'}>
            <input type='text' value={style.extraStroke.color} placeholder='' onChange={this.onChangeExtraStrokeColor} onKeyDown={this.onTextKeyDown}/>
            <div className={'param-color-btn'} style={{backgroundColor: style.extraStroke.color}} onClick={() => { this.setState({extraStrokeColorPicker: !this.state.extraStrokeColorPicker}) }}></div>
          </div>
          {this.state.extraStrokeColorPicker ? <SketchPicker color={style.extraStroke.color} onChange={ this.onPickerChangeExtraStrokeColor } /> : <div></div>}
        </div> : null
      }
      {
        style.extraStroke.enabled ? <div className={'param'}>
          <div className={'param-name'}>Толщина обводки</div>
          <Select value={this.extraStrokeWidthOptions.find((x) => x.value === style.extraStroke.width)} options={this.extraStrokeWidthOptions} classNamePrefix={'map-report'} onChange={this.onChangeExtraStrokeWidth}/>
        </div> : null
      }
    </div>;
  }
  onChangeExtraStrokeEnabled = (checked, e, i) => {
    this.updateFeatureStyle({
      extraStroke: {
        enabled: checked,
      },
    });
  };
  onPickerChangeExtraStrokeColor = (color) => {
    this.updateFeatureStyle({
      extraStroke: {
        color: 'rgba(' + color.rgb.r + ', ' + color.rgb.g + ', ' + color.rgb.b + ', ' + color.rgb.a + ')',
      },
    });
  };
  onChangeExtraStrokeColor = (event) => {
    this.setState({
      extraStrokeColorPicker: false,
    });

    this.updateFeatureStyle({
      extraStroke: {
        color: event.target.value,
      },
    });
  };
  onChangeExtraStrokeWidth = (option) => {
    this.updateFeatureStyle({
      extraStroke: {
        width: parseInt(option.value),
      },
    });
  };

  renderFillParams(sectionName) {
    const {style} = this.state;

    const fillOptions = [
      { value: 'solid', label: 'Сплошная' },
      { value: 'horizontal', label: 'Горизонтальные линии' },
      { value: 'vertical', label: 'Вертикальные линии' },
      { value: 'inclined', label: 'Наклонные линии' },
    ];

    return <div className={'section'}>
      {sectionName ? <div className={'section-name'}>{sectionName}</div> : null}
      {sectionName ? <div className={'section-name-line'}></div> : null}
      {sectionName ? <div className={'section-name-space'}></div> : null}
      <div className={'param'}>
        <div className={'param-name'}>Тип заливки</div>
        <Select value={fillOptions.find((x) => x.value === style.fill.type)} options={fillOptions} classNamePrefix={'map-report'} onChange={this.onChangeFillType}/>
      </div>
      <div className={'param'}>
        <div className={'param-name'}>Цвет заливки</div>
        <div className={'param-color'}>
          <input type='text' value={style.fill.color} placeholder='' onChange={this.onChangeFillColor} onKeyDown={this.onTextKeyDown}/>
          <div className={'param-color-btn'} style={{backgroundColor: style.fill.color}} onClick={() => { this.setState({fillColorPicker: !this.state.fillColorPicker}) }}></div>
        </div>
        {this.state.fillColorPicker ? <SketchPicker color={style.fill.color} onChange={ this.onPickerChangeFillColor } /> : <div></div>}
      </div>
    </div>;
  }
  onChangeFillColor = (event) => {
    this.updateFeatureStyle({
      fill: {
        color: event.target.value,
      },
    });
  };
  onPickerChangeFillColor = (color) => {
    this.updateFeatureStyle({
      fill: {
        color: 'rgba(' + color.rgb.r + ', ' + color.rgb.g + ', ' + color.rgb.b + ', ' + color.rgb.a + ')',
      },
    });
  };
  onChangeFillType = (option) => {
    this.updateFeatureStyle({
      fill: {
        type: option.value,
      },
    });
  };

  renderOrderParams() {
    return <div>
      <div className={'section'}>
        <div className={'param'} data-order={this.state.style.zIndex}>
          <div className={'btn first order-top'} onClick={this.onChangeOrderTop} title={'Вверх'}>
            <IcoLayerToFront />
          </div>
          <div className={'btn order-up'} onClick={this.onChangeOrderUp} title={'Выше'}>
            <IcoLayerUp />
          </div>
          <div className={'btn order-down'} onClick={this.onChangeOrderDown} title={'Ниже'}>
            <IcoLayerDown />
          </div>
          <div className={'btn last order-bottom'} onClick={this.onChangeOrderBottom} title={'Вниз'}>
            <IcoLayerToBottom />
          </div>
          <div className={'clear'}></div>
        </div>
      </div>
    </div>;
  }
  onChangeOrderTop = (event) => {
    this.updateFeatureStyle({
      zIndex: this.getMaxZIndex() + 1,
    });
  };
  onChangeOrderUp = (event) => {
    this.updateFeatureStyle({
      zIndex: this.state.style.zIndex + 1,
    });
  };
  onChangeOrderDown = (event) => {
    const {style} = this.state;
    this.updateFeatureStyle({
      zIndex: style.zIndex - 1,
    });
  };
  onChangeOrderBottom = (event) => {
    const {selectedFeature, style} = this.state;

    const minZIndex = this.getMinZIndex(selectedFeature.getId());
    if (minZIndex === style.zIndex) {
      return;
    }
    this.updateFeatureStyle({
      zIndex: minZIndex - 1,
    });
  };
  getMaxZIndex = (id) => {
    const arr = this.editorCollection.getArray().filter(x => x.getId() !== id).map(x => x.get('style').zIndex);
    if (arr.length === 0) {
      return 0;
    }

    return Math.max.apply(null, arr);
  };
  getMinZIndex = (id) => {
    const arr = this.editorCollection.getArray().filter(x => x.getId() !== id).map(x => x.get('style').zIndex);
    if (arr.length === 0) {
      return 0;
    }

    return Math.min.apply(null, arr);
  };

  onTextKeyDown = (event) => {
    if (event.keyCode === 46) {
      document.removeEventListener('keydown', this.onDocumentKeyDown);
      event.stopPropagation();

      setTimeout(() => {
        document.addEventListener('keydown', this.onDocumentKeyDown);
      }, 500);
    }
  };


  getFeatureStyle = (olFeature) => {
    let style = olFeature.get('style');
    if (!style) {
      style = {
        zIndex: this.getMaxZIndex() + 1,
        text: {
          value: '',
          font: 'sans-serif',
          size: 12,
          color: 'rgba(66, 134, 244, 1)',
          bold: false,
          italic: false,
        },
        stroke: {
          color: 'rgba(66, 134, 244, 1)',
          width: 2,
          type: 'solid',
        },
        extraStroke: {
          enabled: false,
          color: 'rgba(255, 255, 255, 1)',
          width: 1,
        },
        fill: {
          color: 'rgba(66, 134, 244, 0.2)',
          type: 'solid',
        },
        marker: {
          id: this.markers.length === 0 ? null : this.markers[0].id,
          title: '',
          description: '',
          view: 'none',
          content: 'none',
          size: 32,
          number: '',
          color: 'rgba(255, 147, 30, 1)',
          contentColor: 'rgba(82, 91, 102, 1)',
          fill: false,
        },
        arrow: {
          begin: false,
          end: true,
          beginSize: 1,
          endSize: 1,
          equalsSize: true,
        }
      };

      switch (olFeature.get('type')) {
        case 'Line':
        case 'Polyline': {
          delete style.fill;
          delete style.text;
          delete style.marker;
          delete style.arrow;
          break;
        }
        case 'Arrow': {
          delete style.fill;
          delete style.text;
          delete style.marker;
          break;
        }
        case 'Polygon':
        case 'Ellipse':
        case 'Circle':
        case 'Rectangle': {
          delete style.text;
          delete style.circle;
          delete style.arrow;
          break;
        }
        case 'Text': {
          delete style.stroke;
          delete style.fill;
          delete style.marker;
          delete style.arrow;
          break;
        }
        case 'Marker': {
          delete style.stroke;
          delete style.extraStroke;
          delete style.fill;
          delete style.text;
          delete style.arrow;
          break;
        }
      }

      olFeature.set('style', style);
    }

    return style;
  };

  updateFeatureStyle = (update) => {
    const {selectedFeature} = this.state;

    const style = this.getFeatureStyle(selectedFeature);
    lodash.merge(style, update);

    this.setState({
      style,
    });

    this.forceUpdate();

    this.editorSource.dispatchEvent('change');
    this.onFeatureChanged(selectedFeature);
  };

  buildFeatureStyle = (feature) => {
    const featureStyle = this.getFeatureStyle(feature);

    const styles = [];
    let styleOptions = {
      zIndex: featureStyle.zIndex,
    };

    if (featureStyle.fill) {
      styleOptions.fill = new olFill({
        color: this.buildFeatureStyleFillColor(featureStyle.fill),
      });
    }
    if (featureStyle.extraStroke && featureStyle.extraStroke.enabled && featureStyle.extraStroke.width !== 0) {
      styles.push(new olStyle({
        stroke: new olStroke({
          color: featureStyle.extraStroke.color,
          width: (featureStyle.stroke ? featureStyle.stroke.width : 0) + featureStyle.extraStroke.width,
          lineDash: this.buildFeatureStyleLineDash(featureStyle.stroke),
        }),
        zIndex: featureStyle.zIndex,
      }));
    }
    if (featureStyle.stroke) {
      styleOptions.stroke = new olStroke({
        color: featureStyle.stroke.color,
        width: featureStyle.stroke.width,
        lineDash: this.buildFeatureStyleLineDash(featureStyle.stroke),
      });
    }
    if (featureStyle.text) {
      styleOptions.text = new olText({
        text: featureStyle.text.value,
        font: (featureStyle.text.bold ? 'bold ' : '') + (featureStyle.text.italic ? 'italic ' : '') + featureStyle.text.size + 'px ' + featureStyle.text.font,
        stroke: featureStyle.extraStroke && featureStyle.extraStroke.enabled && featureStyle.extraStroke.width !== 0 ? new olStroke({
          color: featureStyle.extraStroke.color,
          width: featureStyle.extraStroke.width,
        }) : null,
        fill: new olFill({
          color: featureStyle.text.color,
        }),
      });
    }

    if (featureStyle.marker && featureStyle.marker.id !== null) {
      styleOptions.image = this.buildFeatureStyleImage(featureStyle.marker, styles);
    }

    const geometry = feature.getGeometry();

    const type = feature.get('type');
    if (type === 'Arrow') {
      const length = geometry.getLength();

      if (featureStyle.arrow.begin) {
        const l = 10000 * featureStyle.arrow.beginSize * featureStyle.stroke.width * (window.OL.getView().getResolution() / 1222.99245256282);
        geometry.forEachSegment((start, end) => this.buildFeatureStyleArrowSegment(styles, featureStyle, l, end, start));
      }
      if (featureStyle.arrow.end ) {
        const l = 10000 * featureStyle.arrow.endSize * featureStyle.stroke.width * (window.OL.getView().getResolution() / 1222.99245256282);
        geometry.forEachSegment((start, end) => this.buildFeatureStyleArrowSegment(styles, featureStyle, l, start, end));
      }
    }

    const style = new olStyle(styleOptions);

    if (!this.state.selectedFeature || this.state.selectedFeature !== feature) {
      styles.push(style);
      return styles;
    }

    styles.push(style);

    const vertexStyleOptions = {
      image: new olCircleStyle({
        radius: 3,
        fill: new olFill({
          color: 'orange'
        })
      }),
      zIndex: 99999,
    };

    if (featureStyle.marker && (featureStyle.marker.view !== 'none' || featureStyle.marker.content !== 'none')) {
      delete vertexStyleOptions.image;
    }

    switch (geometry.getType()) {
      case 'Point': {
        styles.push(new olStyle(vertexStyleOptions));
        break;
      }
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

  buildFeatureStyleFillColor = (fill) => {
    switch (fill.type) {
      case 'solid': {
        return fill.color;
      }
      default: {
        return (function() {
          const pixelRatio = 2;
          const canvas = document.createElement('canvas');
          const context = canvas.getContext('2d');

          canvas.width = 8 * pixelRatio;
          canvas.height = 8 * pixelRatio;

          switch (fill.type) {
            case 'horizontal': {
              // Горизонтальные
              context.beginPath();
              context.lineWidth = 2;
              context.strokeStyle = fill.color;
              context.moveTo(0, 0);
              context.lineTo(canvas.height, 0);
              context.stroke();
              break;
            }
            case 'vertical': {
              // Вертикальные
              context.beginPath();
              context.lineWidth = 2;
              context.strokeStyle = fill.color;
              context.moveTo(0, 0);
              context.lineTo(0, canvas.width);
              context.stroke();
              break;
            }
            case 'inclined': {
              // Наклонные
              canvas.width = 32;
              canvas.height = 16;
              const x0 = 36;
              const x1 = -4;
              const y0 = -2;
              const y1 = 18;
              const offset = 32;
              context.beginPath();
              context.lineWidth = 1;
              context.strokeStyle = fill.color;
              context.moveTo(x0, y0);
              context.lineTo(x1, y1);
              context.moveTo(x0 - offset, y0);
              context.lineTo(x1 - offset, y1);
              context.moveTo(x0 + offset, y0);
              context.lineTo(x1 + offset, y1);
              context.stroke();
              break;
            }
          }

          return context.createPattern(canvas, 'repeat');
        }());
      }
    }
  };

  buildFeatureStyleLineDash = (stroke) => {
    if (!stroke) {
      return null;
    }

    const w = stroke.width * 5;

    switch (stroke.type) {
      case 'solid': {
        return null;
      }
      case 'dotted': {
        return [w, w];
      }
      case 'dot-and-dash': {
        return [w, w, 1, w];
      }
    }
  };

  buildFeatureStyleArrowSegment = (styles, featureStyle, arrowLength, start, end) => {
    const dx = end[0] - start[0];
    const dy = end[1] - start[1];
    const rotation = Math.atan2(dy, dx);

    const polygon = new olPolygon([[
      end,
      [end[0] - arrowLength, end[1] + arrowLength/3],
      [end[0] - arrowLength/1.25, end[1]],
      [end[0] - arrowLength, end[1] - arrowLength/3],
      end
    ]]);
    polygon.rotate(rotation, end);

    const color = colorParse(featureStyle.stroke.color);
    const polygonColor = 'rgba(' + color.values[0] + ', ' + color.values[1] + ', ' + color.values[2] + ', 1)';

    const stroke = new olStroke({
      color: polygonColor,
      width: featureStyle.stroke.width,
    });
    const fill = new olFill({
      color: polygonColor,
    });

    if (featureStyle.extraStroke && featureStyle.extraStroke.enabled && featureStyle.extraStroke.width !== 0) {
      styles.push(new olStyle({
        geometry: polygon,
        stroke: new olStroke({
          color: featureStyle.extraStroke.color,
          width: featureStyle.stroke.width + featureStyle.extraStroke.width,
        }),
        zIndex: featureStyle.zIndex,
      }));
    }

    styles.push(new olStyle({
      zIndex: featureStyle.zIndex + 1,
      geometry: polygon,
      stroke: stroke,
      fill: fill,
    }));
  };

  buildFeatureStyleImage = (markerStyle, styles) => {
    if (markerStyle.view === 'none' && markerStyle.content === 'none') {
      return null;
    }

    const getMarkerSize = (view, content, size) => {
      if (view === 'pin' && content !== 'none') {
        return {
          width: size,
          height: size * 1.18,
          fontSize: '12pt',
        };
      }

      return {
        width: size,
        height: size,
        fontSize: '12pt',
      };
    };
    const markerSize = getMarkerSize(markerStyle.view, markerStyle.content, markerStyle.size);

    const canvas = document.createElement('canvas');
    canvas.width = markerSize.width;
    canvas.height = markerSize.height;
    const ctx = canvas.getContext('2d');

    const drawImageOptions = {
      x: 0,
      y: 0,
      width: 0,
      height: 0,
    };
    const drawTextOptions = {
      x: 0,
      y: 0,
    };
    let anchor = [0.5, 0.5];

    switch (markerStyle.view) {
      case 'none': {
        drawImageOptions.x = 0;
        drawImageOptions.y = 0;
        drawImageOptions.width = canvas.width;
        drawImageOptions.height = canvas.height;

        drawTextOptions.x = canvas.width/2;
        drawTextOptions.y = canvas.height/2 + 1;

        break;
      }
      case 'pin': {
        const svgCanvas = this.buildPinCanvas(markerStyle, canvas.width, canvas.height);
        if (svgCanvas) {
          ctx.drawImage(svgCanvas, 0, 0);
        }

        drawImageOptions.width = canvas.width/2;
        drawImageOptions.height = canvas.width/2;
        drawImageOptions.x = (canvas.width - drawImageOptions.width)/2;
        drawImageOptions.y = (canvas.width - drawImageOptions.height)/2;

        drawTextOptions.x = canvas.width/2;
        drawTextOptions.y = canvas.height/2.15;

        anchor = [0.5, 0.9];

        break;
      }
      case 'point': {
        const r = markerSize.width / 2;

        styles.push(new olStyle({
          image: new olCircleStyle({
            radius: r,
            fill: new olFill({
              color: markerStyle.color,
            })
          }),
        }));
        styles.push(new olStyle({
          image: new olCircleStyle({
            radius: r - 2,
            fill: new olFill({
              color: 'rgba(255, 255, 255, 1)',
            })
          }),
        }));
        styles.push(new olStyle({
          image: new olCircleStyle({
            radius: r - 2 - 2,
            fill: new olFill({
              color: markerStyle.color,
            })
          }),
        }));

        return null;
      }
      case 'circle': {
        const lineWidth = 2;

        const centerX = canvas.width / 2;
        const centerY = canvas.height / 2;
        const radius = (canvas.width / 2) - lineWidth;

        ctx.beginPath();
        ctx.arc(centerX, centerY, radius, 0, 2 * Math.PI, false);
        ctx.fillStyle = markerStyle.fill ? markerStyle.color : '#ffffff';
        ctx.fill();
        ctx.lineWidth = lineWidth;
        ctx.strokeStyle = markerStyle.color;
        ctx.stroke();

        drawImageOptions.x = canvas.width/4;
        drawImageOptions.y = canvas.height/4;
        drawImageOptions.width = canvas.width/2;
        drawImageOptions.height = canvas.height/2;

        drawTextOptions.x = canvas.width/2;
        drawTextOptions.y = canvas.height/2 + 1.2;

        break;
      }
      case 'square': {
        const svgCanvas = this.buildSquareCanvas(markerStyle, canvas.width, canvas.height);
        if (svgCanvas) {
          ctx.drawImage(svgCanvas, 0, 0);
        }

        drawImageOptions.x = 6;
        drawImageOptions.y = 6;

        if (markerStyle.size < 20) {
          drawImageOptions.x = 3;
          drawImageOptions.y = 3;
        }

        drawImageOptions.width = canvas.width - drawImageOptions.x * 2;
        drawImageOptions.height = canvas.height - drawImageOptions.y * 2;

        drawTextOptions.x = canvas.width / 2;
        drawTextOptions.y = canvas.height / 2 + 1.2;

        break;
      }
    }

    switch (markerStyle.content) {
      case 'number': {
        ctx.fillStyle = markerStyle.contentColor;
        ctx.font = markerSize.fontSize + ' sans-serif';
        ctx.textBaseline = 'middle';
        ctx.textAlign = 'center';
        ctx.fillText(markerStyle.number, drawTextOptions.x, drawTextOptions.y);

        break;
      }
      case 'icon': {
        const marker = this.markers.find(x => x.id === markerStyle.id);
        if (marker) {
          if (marker.markerType === 'RASTER') {
            const imageOnLoad = (event) => {
              this.markerImages.push({
                imageUrl: event.target.src,
                content: event.target,
              });

              this.redrawLayer(this.editorLayer);
            };
            const getImage = (link) => {
              if (!this.markerImages) {
                this.markerImages = [];
              }

              const imageUrl = this.props.gisAdapter.getMarkerUrl(link);
              const markerImage = this.markerImages.find(x => x.imageUrl === imageUrl);
              if (markerImage) {
                return markerImage.content;
              }

              const img = new Image();
              img.onload = imageOnLoad;
              img.crossOrigin = "Anonymous";
              img.src = imageUrl;

              return null;
            };

            const image = getImage(marker.link);
            if (image) {
              ctx.drawImage(image, drawImageOptions.x, drawImageOptions.y, drawImageOptions.width, drawImageOptions.height);
            }
          }

          if (marker.markerType === 'VECTOR') {
            const svgCanvas = this.buildCanvas(markerStyle, drawImageOptions.width, drawImageOptions.height);
            if (svgCanvas) {
              ctx.drawImage(svgCanvas, drawImageOptions.x, drawImageOptions.y);
            }
          }
        }

        break;
      }
    }

    return new olIcon({
      img: canvas,
      imgSize: [canvas.width, canvas.height],
      scale: 1,
      anchor,
    });
  };

  buildCanvas = (markerStyle, width, height) => {
    const marker = this.markers.find(x => x.id === markerStyle.id);
    if (!marker) {
      return null;
    }

    if (marker.markerType !== 'VECTOR') {
      return null;
    }

    const getIconSvgContent = (link) => {
      if (!this.markerImages) {
        this.markerImages = [];
      }

      const imageUrl = this.props.gisAdapter.getMarkerUrl(link);
      const markerImage = this.markerImages.find(x => x.imageUrl === imageUrl);
      if (markerImage) {
        return markerImage.content;
      }

      fetch(imageUrl)
        .then(response => response.text())
        .then(x => {
          this.markerImages.push({
            imageUrl: imageUrl,
            content: x.replace(/fill/g,'fill2'),
          });

          this.redrawLayer(this.editorLayer);
        });

      return null;
    };

    const svgContent = getIconSvgContent(marker.link);
    if (!svgContent) {
      return null;
    }

    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;
    const ctx = canvas.getContext('2d');

    ctx.fillStyle = markerStyle.contentColor;
    ctx.drawSvg(svgContent, 0, 0, width, height);

    return canvas;
  };

  buildPinCanvas = (markerStyle, width, height) => {
    const stage = new Konva.Stage({
      container: document.createElement('div'),
      width: width,
      height: height
    });

    const layer = new Konva.Layer({
    });

    if (markerStyle.content === 'none') {
      layer.add(new Konva.Path({
        x: 0,
        y: 0,
        scaleX: width / 32,
        scaleY: height / 32,
        data: 'M16,8a4,4,0,1,0,4,4A4,4,0,0,0,16,8Zm8,4c0,4.42-8,16-8,16S8,16.42,8,12a8,8,0,0,1,16,0Z',
        fill: markerStyle.color,
      }));
    }
    else {
      const scaleX = width / 44;
      const scaleY = height / 52;

      layer.add(new Konva.Path({
        x: 0,
        y: 0,
        scaleX: scaleX,
        scaleY: scaleY,
        data: 'M22,52C22,46.58,0,41,0,22a22,22,0,0,1,44,0C44,41,22,46.58,22,52Z',
        fill: markerStyle.color,
      }));

      layer.add(new Konva.Circle({
        x: width/2,
        y: width/2,
        scaleX: scaleX,
        scaleY: scaleY,
        radius: 20,
        fill: (markerStyle.fill ? markerStyle.color : '#fff'),
      }));
    }

    stage.add(layer);
    layer.draw();

    return layer.getCanvas()._canvas;
  };

  buildSquareCanvas = (markerStyle, width, height) => {
    const stage = new Konva.Stage({
      container: document.createElement('div'),
      width: width,
      height: height
    });

    const layer = new Konva.Layer({
    });

    layer.add(new Konva.Rect({
      width: width,
      height: height,
      fill: markerStyle.color,
      cornerRadius: 3,
    }));
    layer.add(new Konva.Rect({
      x: 2,
      y: 2,
      width: width - 4,
      height: height - 4,
      fill: (markerStyle.fill ? markerStyle.color : '#fff'),
    }));

    stage.add(layer);
    layer.draw();

    return layer.getCanvas()._canvas;
  };


  onEditStarted = () => {
    if (this.state.drawMode) {
      return;
    }

    this.createdFeatureId = -1;

    this.editorCollection.clear();
    this.map.addLayer(this.editorLayer);

    this.translateCollection.clear();
    this.translateInteraction = new olTranslate({
      features: this.translateCollection,
    });
    this.translateInteraction.on('translatestart', (e) => {
      const olFeature = e.features.getArray()[0];
      olFeature.set('prevGeometry', olFeature.getGeometry().clone());
      olFeature.set('initialGeometry', olFeature.getGeometry().clone());

      if (featuresHelper.isPolygonFeature(olFeature)) {
        this.snapCollection.clear();
      }
    });
    this.translateInteraction.on('translating', (e) => {
      const olFeature = e.features.getArray()[0];
      const prevGeometry = olFeature.getGeometry().clone();

      olFeature.set('prevGeometry', prevGeometry);
    });
    this.translateInteraction.on('translateend', (e) => {
      const olFeature = e.features.getArray()[0];

      if (featuresHelper.isPolygonFeature(olFeature)) {
        this.rebuildSnapCollection();
      }

      this.onFeatureChanged(olFeature);
    });
    this.map.addInteraction(this.translateInteraction);

    this.modifyCollection.clear();
    this.modifyInteraction = new olModify({
      features: this.modifyCollection,
      insertVertexCondition: (e) => {
        const feature = this.editorSource.getClosestFeatureToCoordinate(e.coordinate);
        switch (feature.get('type')) {
          case 'Line':
          case 'Arrow':
          case 'Rectangle': {
            return false;
          }
        }
        return feature === this.state.selectedFeature;
      },
    });
    this.modifyInteraction.on('modifystart', (e) => {
      const olFeature = e.features.getArray()[0];
      olFeature.set('prevGeometry', olFeature.getGeometry().clone());
      olFeature.set('initialGeometry', olFeature.getGeometry().clone());
    });
    this.modifyInteraction.on('modifyend', (e) => {
      const olFeature = e.features.getArray()[0];
      this.onFeatureChanged(olFeature);
    });
    this.map.addInteraction(this.modifyInteraction);

    this.snapInteraction = new olSnap({
      features: this.snapCollection,
    });
    this.map.addInteraction(this.snapInteraction);

    this.setState({
      drawMode: true,
    });

    //this.map.selectSuppress();
  };

  onEditFinished = () => {
    this.setState({
      selectedFeature: null,
      style: null,
    }, () => {
      this.rebuildInteractionCollections();
      this.redrawLayer(this.editorLayer);
    });

    return;

    if (!this.state.drawMode) {
      return;
    }

    //this.map.removeLayer(this.editorLayer);
    this.map.removeInteraction(this.modifyInteraction);
    this.map.removeInteraction(this.translateInteraction);
    this.map.removeInteraction(this.snapInteraction);
    this.map.removeInteraction(this.drawInteraction);

    //this.editorCollection.clear();
    this.translateCollection.clear();
    this.modifyCollection.clear();

    this.setState({
      drawMode: false,
    });

    //this.map.selectUnSuppress();
  };


  onButtonClick = (type) => {
    this.map.removeInteraction(this.drawInteraction);
    this.map.removeInteraction(this.snapInteraction);

    const {drawType} = this.state;
    if (drawType === type) {
      this.setState({
        drawType: null,
      });
      return;
    }

    switch (type) {
      case 'Polygon': {
        this.drawInteraction = new olDraw({
          features: this.editorCollection,
          type: 'Polygon',
        });
        break;
      }
      case 'Polyline': {
        this.drawInteraction = new olDraw({
          features: this.editorCollection,
          type: 'LineString',
          maxPoints: Infinity,
        });
        break;
      }
      case 'Line': {
        this.drawInteraction = new olDraw({
          features: this.editorCollection,
          type: 'LineString',
          maxPoints: 2,
        });
        break;
      }
      case 'Ellipse': {
        this.drawInteraction = new olDraw({
          features: this.editorCollection,
          type: 'Circle',
          geometryFunction: (coordinates, geometry) => {
            const center = coordinates[0];
            const last = coordinates[1];
            const dx = center[0] - last[0];
            const dy = center[1] - last[1];
            const radius = Math.sqrt(dx * dx + dy * dy);
            const circle = new olCircle(center, radius);
            const polygon = fromCircle(circle, 64);
            polygon.scale(dx/radius, dy/radius);
            if (!geometry) {
              geometry = polygon;
            } else {
              geometry.setCoordinates(polygon.getCoordinates());
            }
            return geometry
          }
        });
        break;
      }
      case 'Circle': {
        this.drawInteraction = new olDraw({
          features: this.editorCollection,
          type: 'Circle',
        });
        break;
      }
      case 'Rectangle': {
        this.drawInteraction = new olDraw({
          features: this.editorCollection,
          type: 'Circle',
          geometryFunction: createBox(),
        });
        break;
      }
      case 'Arrow': {
        this.drawInteraction = new olDraw({
          features: this.editorCollection,
          type: 'LineString',
          maxPoints: 2,
        });
        break;
      }
      case 'Text': {
        this.drawInteraction = new olDraw({
          features: this.editorCollection,
          type: 'Point',
        });
        break;
      }
      case 'Marker': {
        this.drawInteraction = new olDraw({
          features: this.editorCollection,
          type: 'Point',
        });
        break;
      }
    }

    this.drawInteraction.on('drawstart', (e) => {
      this.onSelectFeature(null);
    });
    this.drawInteraction.on('drawend', (e) => {
      const olFeature = e.feature;

      olFeature.set('type', type);
      olFeature.set('style', this.getFeatureStyle(olFeature));
      this.onFeatureCreated(olFeature);

      setTimeout(() => {
        this.onFeatureChanged(olFeature);

        this.map.removeInteraction(this.drawInteraction);
        for (let olFeature of this.editorCollection.getArray().filter((x) => !x.getGeometry())) {
          this.editorCollection.remove(olFeature);
        }

        setTimeout(() => {
          this.onSelectFeature({
            featureId: olFeature.getId(),
          });
        }, 250);

        this.setState({
          drawType: null,
        });
      }, 100);
    });

    this.map.addInteraction(this.drawInteraction);
    this.map.addInteraction(this.snapInteraction);

    this.setState({
      drawType: type,
      selectedFeature: null,
      style: null,
    }, () => {
      this.rebuildInteractionCollections();
      this.redrawLayer(this.editorLayer);
    });
  };


  onFeatureCreated = (olFeature) => {
    const featureId = this.createdFeatureId--;
    olFeature.setId(featureId);
    olFeature.set('id', featureId);
    olFeature.set('editable', true);

    if (this.props.visible === false) {
      return;
    }
    this.rebuildInteractionCollections(olFeature);
  };

  onFeatureChanged = (olFeature) => {
    if (!this.props.onReportChanged) {
      return;
    }

    const geoJSON = new olGeoJSON();
    const geoJSONWriteOptions = {
      //dataProjection: 'EPSG:4326',
      //featureProjection: 'EPSG:900913',
    };

    let geoJsonFeatures = geoJSON.writeFeaturesObject(this.editorCollection.getArray(), geoJSONWriteOptions);
    for (let feature of geoJsonFeatures.features) {
      delete feature.properties.id;
      delete feature.properties.editable;
      delete feature.properties.prevGeometry;
      delete feature.properties.initialGeometry;
    }

    this.props.onReportChanged({
      center: this.map.getView().getCenter(),
      zoom: this.map.getView().getZoom(),
      features: geoJsonFeatures,
    });
  };

  onSelectFeature = (item) => {
    if (this.props.visible === false) {
      return;
    }

    this.displayFeatures();

    if (!item) {
      this.setState({
        selectedFeature: null,
        style: null,
      }, () => {
        this.rebuildInteractionCollections();
        this.redrawLayer(this.editorLayer);
      });
      return;
    }

    let olFeature = this.editorSource.getFeatureById(item.featureId);

    this.rebuildInteractionCollections(olFeature);
    this.redrawLayer(this.editorLayer);
  };
  onDeselectFeatures = () => {
    this.onSelectFeature();
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
    const items = [];

    const onMapFeature = (feature, layer) => {
      const featureId = feature.getProperties().id;
      if (!featureId || items.filter(x => x.featureId === featureId).length !== 0) {
        return;
      }

      const item = {
        feature,
        featureId,
        geometryType: feature.getGeometry().getType(),
      };

      if (item.geometryType === 'Polygon' || item.geometryType === 'MultiPolygon') {
        item.area = feature.getGeometry().getArea();
      }

      items.push(item);
    };
    this.map.forEachFeatureAtPixel(evt.pixel, onMapFeature, {
      hitTolerance: 5,
      layerFilter: (layer) => true
    });

    const points = items.filter((x) => x.geometryType === 'Point');
    if (points.length !== 0) {
      this.onSelectFeature(points[0]);
      return;
    }

    const lines = items.filter((x) => x.geometryType === 'LineString' || x.geometryType === 'MultiLineString');
    if (lines.length !== 0) {
      this.onSelectFeature(lines[0]);
      return;
    }

    const polygons = items
      .filter((x) => x.geometryType === 'Polygon' || x.geometryType === 'MultiPolygon' || x.geometryType === 'Circle')
      .sort((a, b) => (a.area > b.area) ? 1 : ((b.area > a.area) ? -1 : 0));
    if (polygons.length !== 0) {
      this.onSelectFeature(polygons[0]);
      return;
    }

    this.onSelectFeature();
  };
  onMapMoveEnd = (evt) => {
    this.onFeatureChanged();
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

  toggleLayerView = () => {
    if (this.editorLayer) {
      this.editorLayer.setVisible(!this.editorLayer.getVisible());
    }
  };

  getLayerVisible = () => this.editorLayer ? this.editorLayer.getVisible() : false;
  getFeaturesCount = () => this.editorCollection.getLength();


  rebuildInteractionCollections = (olFeature) => {
    this.setState({
      selectedFeature: olFeature,
      style: olFeature ? olFeature.get('style') : null,

      strokeColorPicker: false,
      strokeExtraColorPicker: false,
      extraStrokeColorPicker: false,
      fillColorPicker: false,
      textColorPicker: false,
      markerColorPicker: false,
    });

    this.modifyCollection.clear();
    if (olFeature) {
      this.modifyCollection.push(olFeature);
    }

    this.translateCollection.clear();
    if (olFeature) {
      switch (olFeature.getGeometry().getType()) {
        case 'Circle':
        case 'Polygon':
        case 'MultiPolygon': {
          this.translateCollection.push(olFeature);
          break;
        }
      }
    }

    this.rebuildSnapCollection();
  };

  rebuildSnapCollection = () => {
    this.snapCollection.clear();
    //this.snapCollection.extend(this.editorCollection.getArray());
  };

  redrawLayer = (olLayer) => {
    if (olLayer) {
      olLayer.setStyle(olLayer.getStyle());
    }
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

    if (e.keyCode === 17) {
      const {selectedFeature} = this.state;
      if (selectedFeature && (selectedFeature.get('type') === 'Line' || selectedFeature.get('type') === 'Polyline' || selectedFeature.get('type') === 'Arrow')) {
        this.modifyCollection.clear();

        this.translateCollection.clear();
        this.translateCollection.push(selectedFeature);
      }
    }
  };

  onDocumentKeyUp = (e) => {
    if (e.keyCode === 17) {
      const {selectedFeature} = this.state;
      if (selectedFeature && (selectedFeature.get('type') === 'Line' || selectedFeature.get('type') === 'Polyline' || selectedFeature.get('type') === 'Arrow')) {
        this.modifyCollection.clear();
        this.modifyCollection.push(selectedFeature);

        this.translateCollection.clear();
      }
    }
  };
}

ReportPanel.propTypes = {
  gisAdapter: PropTypes.object,
  visible: PropTypes.bool,
  layersPanelVisible: PropTypes.bool,
  reportJson: PropTypes.any,
  onReportChanged: PropTypes.func,
  layers: PropTypes.oneOfType([
    PropTypes.array,
    PropTypes.bool,
  ]),
};
