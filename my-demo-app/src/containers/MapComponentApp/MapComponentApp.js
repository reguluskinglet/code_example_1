import React from 'react';
import {Provider} from 'react-redux';
import PropTypes from "prop-types";
import MapComponent from '../MapComponent';
import GisAdapter from '../../adapters/GisAdapter';
import GisAdapterContext from '../../adapters/GisAdapterContext';
import configureProj from '../../configureProj';

import './style.scss';

import configureStore from '../../configureStore';

const initialState = {};
const store = configureStore(initialState);
configureProj();

export default class MapComponentApp extends React.Component {
  constructor(props) {
    GisAdapter.setBaseUrl(props.gisBaseUrl);
    GisAdapter.setGeoServerUrl(props.gisGeoServerUrl);
    GisAdapter.setAuthTokenLocalStorageKey(props.authTokenLocalStorageKey);
    GisAdapter.setOnError(props.onError);
    super(props);
  }

  componentDidCatch(error, errorInfo) {
    const {onError} = this.props;
    if (onError) {
      onError({
        message: error,
      });
    }
  }

  render() {
    const {
      zoom, center, mapId, viewMode, configuration, mapDragPanKinetic,
      geoJson, onGeometryChanged, onSelect, onExtentChanged, cardHeaderRender, cardContentRender, cardWidth, cardHeight, reportJson, onReportChanged, extraLayers, clusterClickMode,
      onMapCreated,
    } = this.props;

    const limits = {...this.props.limits};

    return <GisAdapterContext.Provider value={GisAdapter}>
      <Provider store={store}>
        <MapComponent
          gisAdapter={GisAdapter}
          center={center}
          zoom={zoom}
          limits={limits}
          mapId={mapId}
          viewMode={viewMode}
          configuration={configuration}
          geoJson={geoJson}
          onGeometryChanged={onGeometryChanged}
          onSelect={onSelect}
          onExtentChanged={onExtentChanged}
          reportJson={reportJson}
          onReportChanged={onReportChanged}
          cardHeaderRender={cardHeaderRender}
          cardContentRender={cardContentRender}
          cardWidth={cardWidth}
          cardHeight={cardHeight}
          extraLayers={extraLayers}
          mapDragPanKinetic={mapDragPanKinetic}
          clusterClickMode={clusterClickMode}
          onMapCreated={onMapCreated}
        />
      </Provider>
    </GisAdapterContext.Provider>;
  }
}

MapComponentApp.propTypes = {
  gisBaseUrl: PropTypes.string,
  gisGeoServerUrl: PropTypes.string,
  authTokenLocalStorageKey: PropTypes.string,
  onError: PropTypes.func,
  center: PropTypes.array,
  zoom: PropTypes.number,
  limits: PropTypes.object,
  mapId: PropTypes.any,
  configuration: PropTypes.any,
  viewMode: PropTypes.string,
  geoJson: PropTypes.any,
  onGeometryChanged: PropTypes.any,
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