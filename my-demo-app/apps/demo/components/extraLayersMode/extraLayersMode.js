import * as React from 'react';
import {Tabs} from 'antd';
import debounce from "lodash/debounce";

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

const extraLayers = [{
    name: 'Данные',
    url: 'http://geoserver.gis.dev.sac.sphaera.cloud/geoserver/ows?service=wms&version=1.3.0&layers=sphaera:okato_centroids&styles=join_data',
    order: 0,
    visible: true
  },
  {
    name: 'Данные Lat/Lon',
    url: 'http://geoserver.gis.dev.sac.sphaera.cloud/geoserver/ows?service=wms&version=1.3.0&layers=sphaera:external_latlon_layer&styles=join_latlon',
    order: 1,
    visible: false
  },
  {
    name: 'Heatmap(gs)',
    url: 'http://geoserver.gis.dev.sac.sphaera.cloud/geoserver/ows?service=wms&version=1.3.0&layers=sphaera:okato_centroids_3857&styles=join_heatmap_gs',
    order: 2,
    visible: false
  },
  {
    name: 'Heatmap(new)',
    url: 'http://geoserver.gis.dev.sac.sphaera.cloud/geoserver/ows?service=wms&version=1.3.0&layers=sphaera:okato_centroids_3857&styles=join_heatmap',
    order: 3,
    visible: false
  },
  {
    name: 'Choropleth map',
    url: 'http://geoserver.gis.dev.sac.sphaera.cloud/geoserver/ows?service=wms&version=1.3.0&layers=sphaera:okato_districts&styles=join_choropleth',
    order: 4,
    visible: false
  },
  {
    name: 'Proportional map',
    url: 'http://geoserver.gis.dev.sac.sphaera.cloud/geoserver/ows?service=wms&version=1.3.0&layers=sphaera:okato_centroids&styles=join_proportional',
    order: 5,
    visible: false
  }
];

export class ExtraLayersMode extends React.Component<any, any> {
  state = {
    extraLayersJson: JSON.stringify(extraLayers, null, 4),
    key: 1,
  };

  constructor(props) {
    super(props);
    this.onUpdateComponent = debounce(this.onUpdateComponent, 800);
  }

  onChange = (event) => {
    this.setState({
      extraLayersJson: event.target.value,
    });
    this.onUpdateComponent();
  };
  onUpdateComponent = () => {
    this.setState({
      key: this.state.key + 1,
    });
  };

  render() {
    const {extraLayersJson, key} = this.state;

    const mapId = 1007;
    const configuration = {
      visible: ['layers', 'scale'],
    };

    const getExtraLayers = () => {
      try {
        return JSON.parse(extraLayersJson);
      } catch (e) {
        return null;
      }
    };

    return (
      <Tabs defaultActiveKey="1">
        <TabPane tab="Пример" key="1">
          <div style={{position: 'absolute', top: '60px', bottom: '24px', width: '100%'}}>
            <MapComponentApp
              key={key}
              viewMode='full'
              mapId={mapId}
              configuration={configuration}
              gisBaseUrl={GIS_BASE_URL} gisGeoServerUrl={GIS_GEOSERVER_URL} authTokenLocalStorageKey={AUTH_TOKEN_LOCAL_STORAGE_KEY}
              center={center} zoom={zoom} limits={limits}
              extraLayers={getExtraLayers()}
            />
          </div>
        </TabPane>
        <TabPane tab="Интеграция" key="2" style={{overflowY: 'scroll', paddingRight: '10px'}}>
          <p>Дополнительные слои:</p>
          <textarea style={{width: '100%'}} rows='5' value={extraLayersJson} onChange={this.onChange} />

          <p>Код React-компонента:</p>
          <Highlight className='JavaScript'>
            {Code}
          </Highlight>
        </TabPane>
      </Tabs>
    );
  }
}