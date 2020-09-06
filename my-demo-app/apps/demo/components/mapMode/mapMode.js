import * as React from 'react';
import {Tabs} from 'antd';

const TabPane = Tabs.TabPane;
import Highlight from 'react-highlight'
import Code from './demo-code';

import MapComponentApp from '../../../../src/';

import './style.scss';

const GIS_BASE_URL = window.BACKEND_ADDR;
const GIS_GEOSERVER_URL = window.GEOSERVER_ADDR;
const AUTH_TOKEN_LOCAL_STORAGE_KEY = 'AuthToken';
const center = [15213650.715525128, 5747063.475428338];
const zoom = 7;
const limits = {
  maxZoom: 17,
  minZoom: 7
};

export class MapMode extends React.Component<any, any> {
  state = {
    layers: [],
    layerName: '',
    baseLayerUid: '',
  };

  constructor(props) {
    super(props)
  }

  olMap = null;

  onGetLayers = () => {
    const layers = this.olMap.gis.getLayers();
    this.setState({
      layers,
    });
    console.log(layers);
  };

  render() {
    const mapId = 1015;
    const configuration = {
      visible: ['layers', 'scale'],
    };

    const {layers, layerName, baseLayerUid} = this.state;

    return (
      <Tabs defaultActiveKey="1">
        <TabPane tab="Пример" key="1">
          <div style={{position: 'absolute', top: '60px', bottom: '24px', width: '100%'}}>
            <MapComponentApp
              viewMode='full'
              mapId={mapId}
              configuration={configuration}
              gisBaseUrl={GIS_BASE_URL} gisGeoServerUrl={GIS_GEOSERVER_URL} authTokenLocalStorageKey={AUTH_TOKEN_LOCAL_STORAGE_KEY}
              center={center} zoom={zoom} limits={limits}
              onMapCreated={(olMap) => { // Объект ol/Map создан
                console.log(olMap);
                this.olMap = olMap;
              }}
            />
          </div>
        </TabPane>
        <TabPane tab="Интеграция" key="2" style={{overflowY: 'scroll', paddingRight: '10px'}}>
          <p>Код React-компонента:</p>
          <Highlight className='JavaScript'>
            {Code}
          </Highlight>

          <p>olMap.gis.setLayerVisible(name, visible) - изменение видимости слоя</p>
          <p>
            <textarea placeholder={'LayerName (название слоя, свойство layers в объектах массива из getLayers)'} style={{width: '100%'}} rows='1' value={layerName} onChange={(event) => { this.setState({layerName: event.target.value})}} />
            <button onClick={() => this.olMap.gis.setLayerVisible(layerName, true)}>olMap.gis.setLayerVisible(&apos;{layerName}&apos;, true)</button>
            <button onClick={() => this.olMap.gis.setLayerVisible(layerName, false)}>olMap.gis.setLayerVisible(&apos;{layerName}&apos;, false)</button>
          </p>


          <p>olMap.gis.setBaseLayer(uid) - изменение подложки</p>
          <p>
            <textarea placeholder={'BaseLayerUid (uid подложки слоя)'} style={{width: '100%'}} rows='1' value={baseLayerUid} onChange={(event) => { this.setState({baseLayerUid: event.target.value})}} />
            <button onClick={() => this.olMap.gis.setBaseLayer(baseLayerUid)}>olMap.gis.setBaseLayer(&apos;{baseLayerUid}&apos;)</button>
          </p>

          <p>olMap.gis.getLayers() - получение списка слоев <button onClick={this.onGetLayers}>olMap.gis.getLayers()</button></p>
          <Highlight className='JavaScript'>
            {JSON.stringify(layers, null, 2)}
          </Highlight>
        </TabPane>
      </Tabs>
    );
  }
}