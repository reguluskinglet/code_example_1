import React from 'react';
import PropTypes from 'prop-types';
import './style.scss';

import olDragZoom from "ol/interaction/DragZoom";
import Map from '../../components/Map';
import LayersPanel from '../../components/Layers/LayersPanel';
import EditorPanel from '../../components/EditorPanel';
import ReportPanel from '../../components/ReportPanel';
import MeasureTool from "../../components/MeasureTool/MeasureTool";
import MapSearch from "../../components/MapSearch/MapSearch"
import SearchResultsLayer from "../../components/Layers/SearchResultsLayer/SearchResultsLayer";
import MapLayers from "../../components/Layers/MapLayers/MapLayers";
import MapTooltip from "../../components/MapTooltip/MapTooltip";
import MapCard from "../../components/MapCard/MapCard";
import layersHelper from "../../utils/layersHelper";
import TimelapsePanel from "../../components/TimelapsePanel/TimelapsePanel";
import ExportPanel from "../../components/ExportPanel/ExportPanel";

import IcoBrush from '../../icons/ico_brush.svg';
import IcoPointMarker from '../../icons/ico_point marker.svg';
import IcoPolygon from '../../icons/ico_polygon.svg';
import IcoRuler from '../../icons/ico_ruler.svg';
import IcoExport from '../../icons/ico_export.svg';
import IcoPrint from '../../icons/ico_print.svg';
import IcoLayers from '../../icons/ico_layers.svg';
import IcoZoomIn from '../../icons/ico_zoom_in.svg';
import IcoZoomOut from '../../icons/ico_zoom_out.svg';
import IcoLocation from '../../icons/ico_centering.svg';
import IcoReset from '../../icons/ico_centering.svg';
import IcoShowObjects from '../../icons/ico_show_objects.svg';
import IcoHideObjects from '../../icons/ico_hide_objects.svg';

export default class MapComponent extends React.PureComponent { // eslint-disable-line react/prefer-stateless-function
  state = {
    layersPanelVisible: false,
    editorPanelVisible: false,
    reportPanelVisible: false,
    measurePanelVisible: false,
    exportPanelVisible: false,
    dragZoomActive: false,
  };

  olMap = null;
  dragZoomCtr = null;

  componentDidMount() {
    this.dragZoomCtr = new olDragZoom({
      condition: function(mapBrowserEvent) {
        return true;
      }
    });

    this.props.onComponentDidMount();
  };

  componentDidUpdate(prevProps, prevState, snapshot) {
  }

  render() {
    const {
      loading, error, layers, refreshPeriod, mapDragPanKinetic, onMapCreated,
      configuration, viewMode, center, zoom, limits,
      gisAdapter, geoJson, reportJson, cardHeaderRender, cardContentRender, cardWidth, cardHeight,
    } = this.props;
    const {
      layersPanelVisible,
      editorPanelVisible,
      reportPanelVisible,
      measurePanelVisible,
      exportPanelVisible,
    } = this.state;

    const layersListProps = {loading, error, layers, refreshPeriod, center, zoom, limits};

    if (this.olMap) {
      this.olMap.gis = {
        getLayers: () => layersHelper.toArray(layers).filter(x => x.type === 'layer' || x.type === 'base'),
        setLayerVisible: (name, visible) => {
          const layersArray = layersHelper.toArray(layers);
          layersArray.map((item) => {
            if (item.layers === name) {
              item.visible = visible;
            }
          });

          this.props.onChangeLayers(layers);
          if (this.reportPanel) {
            this.reportPanel.onChangeLayers(layers);
          }
          this.forceUpdate();
        },
        setBaseLayer: (uid) => {
          const layersArray = layersHelper.toArray(layers);
          layersArray.map((item) => {
            if (item.type === 'base') {
              item.visible = item.uid === uid;
            }
          });

          this.props.onChangeLayers(layers);
          if (this.reportPanel) {
            this.reportPanel.onChangeLayers(layers);
          }
          this.forceUpdate();
        },
      };
    }

    return (
      <div style={{height: '100%'}} className={'gis-layout ' + viewMode}>
        <div style={{height: '100%', margin: '0', padding: 0, background: '#fff', position: 'relative'}}>
          <LayersPanel
            visible={layersPanelVisible}
            layers={layers}
            onLayersChanged={(layers) => {
              this.props.onChangeLayers(layers);
              if (this.reportPanel) {
                this.reportPanel.onChangeLayers(layers);
              }
              this.forceUpdate();
            }}
            onClose={() => {
              this.setState({
                layersPanelVisible: false,
              });
            }}
          />
          {configuration.visible.indexOf('editor') === -1 ? null :
            <EditorPanel
              ref={(el) => this.editorPanel = el}
              visible={editorPanelVisible}
              layersPanelVisible={layersPanelVisible}
              geoJson={geoJson}
              onGeometryChanged={this.props.onGeometryChanged}
              onCreated={() => this.forceUpdate() }
            />}
          {configuration.visible.indexOf('report') === -1 ? null :
            <ReportPanel
              ref={(el) => this.reportPanel = el}
              gisAdapter={gisAdapter}
              visible={reportPanelVisible}
              layersPanelVisible={layersPanelVisible}
              reportJson={reportJson}
              onReportChanged={this.props.onReportChanged}
              layers={layers}
            />}
          {configuration.visible.indexOf('timelapse') === -1 ? null :
            <TimelapsePanel
              ref={(el) => this.timelapsePanel = el}
              visible={true}
              layers={layers}
              playback={configuration.playback}
            />}
          {configuration.visible.indexOf('search') === -1 ? null :
            <MapSearch
              ref={(el) => this.mapSearch = el}
              gisAdapter={gisAdapter}
              layer={this.searchResultsLayer}
              configuration={configuration.search}
            />}
          <div className={'gis-map-buttons' + (layersPanelVisible ? ' gis-layers-panel-active' : '') + (editorPanelVisible ? ' gis-drawer-panel-active' : '')}>
            {this.renderEditorPointButton()}
            {this.renderEditorPolygonButton()}
            {this.renderReportViewButton()}
            {this.renderReportButton()}
            {configuration.visible.indexOf('measures') === -1 ? null :
              <div>
                {this.renderMeasuresButton()}
                <MeasureTool
                  visible={measurePanelVisible}
                />
              </div>}
            {configuration.visible.indexOf('export') === -1 ? null :
              <div>
                {this.renderExportButton()}
                <ExportPanel
                  visible={exportPanelVisible}
                />
              </div>}
            {this.renderPrintButton()}
            {this.renderLayersButton()}
          </div>
          <div id={'gis-map-info-panel'}></div>
          {configuration.visible.indexOf('scale') === -1 ? null :
            <div className={'gis-scale-buttons' + (layersPanelVisible ? ' gis-layers-panel-active' : '')}>
              {this.renderDragZoomButton()}
              {this.renderZoomInButton()}
              <div className={'gis-scale-delimiter'}/>
              {this.renderZoomOutButton()}
              <div className={'gis-scale-delimiter2'}/>
              {this.renderResetButton()}
            </div>}
          <Map
            gisAdapter={gisAdapter}
            getAliases={!!this.props.onSelect}
            style={{height: "100%"}} {...layersListProps}
            onMapCreated={(olMap) => {
              if (onMapCreated) {
                this.olMap = olMap;
                onMapCreated(this.olMap);
              }
            }}
            onSelect={(click, coordinate, smallerFeature, features, mapCard) => {
              if (this.props.onSelect) {
                this.props.onSelect(click, smallerFeature, features);
              }

              if (!mapCard) {
                this.mapCard.setFeature(coordinate, null);
                return;
              }

              if (click.pointerEvent.button === 0 && !configuration.cardDisabled) {
                this.mapCard.setFeature(coordinate, smallerFeature);
              } else {
                this.mapCard.setFeature(coordinate, null);
              }
            }}
            onExtentChanged={this.props.onExtentChanged}
            dragPanKinetic={mapDragPanKinetic}
            clusterClickMode={this.props.clusterClickMode}
          >
            <MapLayers {...layersListProps}/>
            <MapCard
              ref={(el) => this.mapCard = el}
              gisAdapter = {gisAdapter}
              headerRender = {cardHeaderRender}
              contentRender = {cardContentRender}
              width = {cardWidth}
              height = {cardHeight}
            />
            <SearchResultsLayer
              ref={(el) => this.searchResultsLayer = el}
              onSelected={(item) => {
                this.mapSearch.onMapSelect(item);
              }}
              onFocused={(item) => {
              }}/>
            <MapTooltip layers={layers} />
          </Map>
          <a id="image-download"></a>
        </div>
      </div>
    );
  }

  renderEditorPointButton() {
    if (this.props.configuration.visible.indexOf('editor') === -1) {
      return null;
    }

    const onButtonClick = () => {
      this.editorPanel.onEditStarted();
      this.editorPanel.setDrawType('Point');
      setTimeout(() => this.forceUpdate(), 100);
    };

    const className = 'gis-map-button gis-editor-button' + (this.editorPanel && this.editorPanel.getDrawType() === 'Point' ? ' gis-map-button-active' : '');

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={''}>
      <span className={'icon'}>
        <IcoPointMarker />
      </span>
    </a>;
  }
  renderEditorPolygonButton() {
    if (this.props.configuration.visible.indexOf('editor') === -1) {
      return null;
    }

    const onButtonClick = () => {
      this.editorPanel.onEditStarted();
      this.editorPanel.setDrawType('Polygon');
      setTimeout(() => this.forceUpdate(), 100);
    };

    const className = 'gis-map-button gis-editor-button' + (this.editorPanel && this.editorPanel.getDrawType() === 'Polygon' ? ' gis-map-button-active' : '');

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={''}>
      <span className={'icon'}>
        <IcoPolygon />
      </span>
    </a>;
  }
  renderReportViewButton() {
    if (this.props.configuration.visible.indexOf('report') === -1) {
      return null;
    }

    const onButtonClick = () => {
      this.reportPanel.toggleLayerView();
      this.forceUpdate();
    };

    const layerVisible = this.reportPanel && this.reportPanel.getLayerVisible();
    const className = 'gis-map-button gis-report-view-button' + (layerVisible ? ' gis-map-button-active' : '');

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={layerVisible ? 'Скрыть' : 'Отобразить'}>
      <span className={'icon'}>
        {layerVisible ? <IcoShowObjects /> : <IcoHideObjects />}
      </span>
    </a>;
  }
  renderReportButton() {
    if (this.props.configuration.visible.indexOf('report') === -1) {
      return null;
    }

    const onButtonClick = () => {
      this.map.selectToggleSuppress(!this.state.reportPanelVisible);
      this.setState({
        reportPanelVisible: !this.state.reportPanelVisible,
      });
    };

    const { reportPanelVisible } = this.state;
    const className = 'gis-map-button gis-report-button' + (reportPanelVisible ? ' gis-map-button-active' : '');

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={''}>
      <span className={'icon'}>
        <IcoBrush />
      </span>
    </a>;
  }
  renderMeasuresButton() {
    if (this.props.configuration.visible.indexOf('measures') === -1) {
      return null;
    }

    const onButtonClick = () => {
      this.setState({
        measurePanelVisible: !this.state.measurePanelVisible,
      });
    };

    const { measurePanelVisible } = this.state;
    const className = 'gis-map-button gis-measures-button' + (measurePanelVisible ? ' gis-map-button-active' : '');

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={''}>
      <span className={'icon'}>
        <IcoRuler />
      </span>
    </a>;
  }
  renderExportButton() {
    if (this.props.configuration.visible.indexOf('export') === -1) {
      return null;
    }

    const onButtonClick = () => {
      this.setState({
        exportPanelVisible: !this.state.exportPanelVisible,
      });
    };

    const { exportPanelVisible } = this.state;
    const className = 'gis-map-button gis-down-arrow-button' + (exportPanelVisible ? ' gis-map-button-active' : '');

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={'Экспорт экстента'}>
      <span className={'icon'}>
        <IcoExport />
      </span>
    </a>;
  }
  renderPrintButton() {
    if (this.props.configuration.visible.indexOf('print') === -1) {
      return null;
    }

    const onButtonClick = () => {
      const viewportNode = document.getElementsByClassName('ol-viewport')[0];
      const viewportParentNode = viewportNode.parentNode;

      const printMapContainer = document.createElement("div");
      printMapContainer.id = "printMapContainer";
      printMapContainer.style.width = viewportNode.clientWidth + "px";
      printMapContainer.style.height = viewportNode.clientHeight + "px";

      document.body.appendChild(printMapContainer);
      printMapContainer.appendChild(viewportNode);

      window.print();

      viewportParentNode.appendChild(viewportNode);
      document.body.removeChild(printMapContainer);
    };

    const className = 'gis-map-button gis-print-button';

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={'Печать экстента'}>
      <span className={'icon'}>
        <IcoPrint />
      </span>
    </a>;
  }
  renderLayersButton() {
    if (this.props.configuration.visible.indexOf('layers') === -1) {
      return null;
    }

    const onButtonClick = () => {
      this.setState({
        layersPanelVisible: !this.state.layersPanelVisible,
      });
    };

    const { layersPanelVisible } = this.state;
    const className = 'gis-map-button gis-layers-button' + (layersPanelVisible ? ' gis-map-button-active' : '');

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={'Слои'}>
      <span className={'icon'}>
        <IcoLayers />
      </span>
    </a>;
  }

  renderDragZoomButton() {
    return <span></span>;

    const onButtonClick = () => {
      const {
        dragZoomActive
      } = this.state;

      if (dragZoomActive) {
        this.map.removeInteraction(this.dragZoomCtr);
      } else {
        this.map.addInteraction(this.dragZoomCtr);
      }

      this.setState({
        dragZoomActive: !dragZoomActive,
      });
    };

    const { dragZoomActive } = this.state;
    const className = 'gis-map-button gis-drag-zoom-button' + (dragZoomActive ? ' gis-map-button-active' : '');

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={'Выбор прямоугольной области для масштабирования'}>
      <span className={'icon'}></span>
    </a>;
  }
  renderZoomInButton() {
    const onButtonClick = () => {
      let zoom = this.map.getView().getZoom();
      if (zoom + 1 > this.map.getView().getMaxZoom()) {
        return;
      }
      this.map.getView().animate({
        zoom: zoom + 1,
        duration: 250
      });
    };

    const className = 'gis-map-button gis-zoom-in-button';

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={'Приблизить'}>
      <span className={'icon'}>
        <IcoZoomIn />
      </span>
    </a>;
  }
  renderZoomOutButton() {
    const onButtonClick = () => {
      let zoom = this.map.getView().getZoom();
      if (zoom - 1 < this.map.getView().getMinZoom()) {
        return;
      }
      this.map.getView().animate({
        zoom: zoom - 1,
        duration: 250
      });
    };

    const className = 'gis-map-button gis-zoom-out-button';

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={'Отдалить'}>
      <span className={'icon'}>
        <IcoZoomOut />
      </span>
    </a>;
  }

  renderResetButton() {
    const {zoom, center, layers, onSelect} = this.props;

    const onButtonClick = () => {
      this.map.getView().setCenter(center);
      this.map.getView().setZoom(zoom);

      if (this.mapSearch) {
        this.mapSearch.reset();
      }
      if (this.mapCard) {
        this.mapCard.reset();
      }

      if (onSelect) {
        onSelect(null, null);
      }
    };

    const className = 'gis-map-button gis-reset-button';

    return <a className={className} href="javascript:void(0)" onClick={onButtonClick} title={'Сбросить'}>
      <span className={'icon'}>
        <IcoReset />
      </span>
    </a>;
  }

  get map() {
    return window.OL;
  }
}

MapComponent.propTypes = {
  gisAdapter: PropTypes.object,
  configuration: PropTypes.any,
  viewMode: PropTypes.string,
  center: PropTypes.array,
  zoom: PropTypes.number,
  limits: PropTypes.object,
  loading: PropTypes.bool,
  error: PropTypes.oneOfType([
    PropTypes.object,
    PropTypes.bool,
  ]),
  layers: PropTypes.oneOfType([
    PropTypes.array,
    PropTypes.bool,
  ]),
  refreshPeriod: PropTypes.any,
  onComponentDidMount: PropTypes.func,
  onChangeLayers: PropTypes.func,
  geoJson: PropTypes.any,
  onGeometryChanged: PropTypes.func,
  onSelect: PropTypes.func,
  onExtentChanged: PropTypes.func,
  reportJson: PropTypes.any,
  onReportChanged: PropTypes.func,
  cardHeaderRender: PropTypes.func,
  cardContentRender: PropTypes.func,
  cardWidth: PropTypes.number,
  cardHeight: PropTypes.number,
  extraLayers: PropTypes.array,
  mapDragPanKinetic: PropTypes.object,
  clusterClickMode:  PropTypes.string,
  onMapCreated: PropTypes.func,
};
