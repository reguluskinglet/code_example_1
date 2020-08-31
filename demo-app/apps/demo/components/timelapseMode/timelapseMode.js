import * as React from 'react';
import {Tabs} from 'antd';
import moment from 'moment';

const TabPane = Tabs.TabPane;
import Highlight from 'react-highlight'
import Code from './demo-code';

import MapComponentApp from '../../../../src/';

import './style.scss';

const GIS_BASE_URL = window.BACKEND_ADDR;
const GIS_GEOSERVER_URL = window.GEOSERVER_ADDR;
const AUTH_TOKEN_LOCAL_STORAGE_KEY = 'AuthToken';
const center = [12969187.612909647, 4863580.327491608];
const zoom = 9;
const limits = {
  maxZoom: 17,
  minZoom: 7
};

export class TimelapseMode extends React.Component<any, any> {
  state = {
  };

  constructor(props) {
    super(props)
  }

  render() {
    const mapId = 1041;
    const configuration = {
      visible: ['timelapse', 'layers', 'scale'],
      playback: {
        frequency: 'hours', // частота изменения данных (seconds, minutes. hours, days, weeks, months, years)
        min: moment('13:31 02.02.2008', 'HH:mm DD.MM.YYYY'), // начало абсолютного периода
        max: moment('17:39 08.02.2008', 'HH:mm DD.MM.YYYY'), // окончание абсолютного периода
        //relative: {
        //  type: 'seconds',
        //  value: 60*60*24*30,
        //}, // относительный период
      }
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