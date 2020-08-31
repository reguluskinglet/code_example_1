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

export class EnterMode extends React.Component<any, any> {
  state = {
    geoJson: '',
    changedGeoJson: '',
  };

  constructor(props) {
    super(props)
  }

  render() {
    const {geoJson, changedGeoJson} = this.state;

    const mapId = 1007;
    const configuration = {
      visible: ['search', 'editor', 'measures', 'export', 'print', 'layers', 'scale'],
      search: {
        DADATA: {
          limit: 1,
        },
        YANDEX: {
          limit: 3,
        },
        LAYER: {
          limit: 3,
          layerIds: [1002, 1003],
        },
      },
    };

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
              geoJson={geoJson}
              onGeometryChanged={(geoJson) => {
                this.setState({
                  changedGeoJson: JSON.stringify(geoJson, null, 2)
                });
              }}
            />
          </div>
        </TabPane>
        <TabPane tab="Интеграция" key="2" style={{overflowY: 'scroll', paddingRight: '10px'}}>
          <p>Установка геометрии объектов путем получения из системы:</p>
          <textarea style={{width: '100%'}} rows='5' value={geoJson} onChange={(event) => { this.setState({geoJson: event.target.value})}} />

          <p>Возвращение системе геометрии объектов:</p>
          <Highlight className='JavaScript'>
            {changedGeoJson}
          </Highlight>

          <p>Код React-компонента:</p>
          <Highlight className='JavaScript'>
            {Code}
          </Highlight>
        </TabPane>
      </Tabs>
    );
  }
}