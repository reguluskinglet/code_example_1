import * as React from 'react';
import {Tabs} from 'antd';

const TabPane = Tabs.TabPane;
import Highlight from 'react-highlight'
import Code from './demo-code';

import MapComponentApp from '../../../../src/';

const GIS_BASE_URL = window.BACKEND_ADDR;
const GIS_GEOSERVER_URL = window.GEOSERVER_ADDR;
const AUTH_TOKEN_LOCAL_STORAGE_KEY = 'AuthToken';
const center = [15213650.715525128, 5747063.475428338];
const zoom = 7;
const limits = {
  maxZoom: 17,
  minZoom: 7
};

export class FixedMode extends React.Component<any, any> {
  constructor(props) {
    super(props)
  }

  render() {
    const mapId = 1007;
    const configuration = {
      visible: ['layers', 'scale'],
    };

    return (
      <Tabs defaultActiveKey="1">
        <TabPane tab="Пример" key="1">
          <div style={{width: 470, height: 480}}>
            <MapComponentApp
              viewMode='fixed'
              mapId={mapId}
              configuration={configuration}
              gisBaseUrl={GIS_BASE_URL} gisGeoServerUrl={GIS_GEOSERVER_URL} authTokenLocalStorageKey={AUTH_TOKEN_LOCAL_STORAGE_KEY}
              center={center} zoom={zoom} limits={limits}
            />
          </div>
        </TabPane>
        <TabPane tab="Интеграция" key="2" style={{overflowY: 'scroll', paddingRight: '10px'}}>
          <p>Код React-компонента:</p>
          <Highlight className='JavaScript'>
            {Code}
          </Highlight>
        </TabPane>
      </Tabs>
    );
  }
}