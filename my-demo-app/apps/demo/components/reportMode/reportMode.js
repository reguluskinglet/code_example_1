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

export class ReportMode extends React.Component<any, any> {
  state = {
    reportJson: '',
    changedReportJson: '',
  };

  constructor(props) {
    super(props)
  }

  render() {
    const {reportJson, changedReportJson} = this.state;

    const mapId = 1007;
    const configuration = {
      visible: ['report', 'layers', 'scale', 'print', 'export'],
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
              reportJson={reportJson}
              onReportChanged={(reportJson) => {
                this.setState({
                  changedReportJson: JSON.stringify(reportJson, null, 2)
                });
              }}
            />
          </div>
        </TabPane>
        <TabPane tab="Интеграция" key="2" style={{overflowY: 'scroll', paddingRight: '10px'}}>
          <p>Установка оформления отчета путем получения из системы:</p>
          <textarea
            style={{width: '100%'}} rows='5' value={reportJson} onChange={(event) => { this.setState({
              reportJson: event.target.value,
            })}} />

          <p>Возвращение системе оформления отчета:</p>
          <Highlight className='JavaScript'>
            {changedReportJson}
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