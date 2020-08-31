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

export class SelectMode extends React.Component<any, any> {
  state = {
    selected: null,
  };

  constructor(props) {
    super(props)
  }

  render() {
    const {selected} = this.state;

    const mapId = 1007;
    const configuration = {
      //cardDisabled: true,
      visible: ['search', /*'measures', 'export', 'print', */'layers', 'scale', 'select'],
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
              onSelect={(click, feature) => {
                console.log(click);
                console.log(feature);
                this.setState({selected: {
                  click,
                  feature
                }});
              }}
            />
          </div>
        </TabPane>
        <TabPane tab="Интеграция" key="2" style={{overflowY: 'scroll', paddingRight: '10px'}}>
          <p>Кнопка мыши click.pointerEvent.button</p>
          <Highlight className='JavaScript'>
            {selected && selected.click && selected.click.pointerEvent ? selected.click.pointerEvent.button : ''}
          </Highlight>

          <p>Координаты клика click.lat, click.lon</p>
          <Highlight className='JavaScript'>
            {selected && selected.click ? (selected.click.lat + ', ' + selected.click.lon) : ''}
          </Highlight>

          <p>Экранные координаты клика click.pointerEvent.x, click.pointerEvent.y</p>
          <Highlight className='JavaScript'>
            {selected && selected.click ? (selected.click.pointerEvent.x + ', ' + selected.click.pointerEvent.y) : ''}
          </Highlight>

          <p>Идентификатор слоя feature.layerId</p>
          <Highlight className='JavaScript'>
            {selected && selected.feature  ? selected.feature.layerId : ''}
          </Highlight>

          <p>Идентификатор объекта feature.id</p>
          <Highlight className='JavaScript'>
            {selected && selected.feature ? selected.feature.id : ''}
          </Highlight>

          <p>Свойства объекта feature.properties</p>
          <Highlight className='JavaScript'>
            {selected && selected.feature && selected.feature.properties ? JSON.stringify(selected.feature.properties, null, 2) : null}
          </Highlight>

          <p>Алиасы объекта feature.aliases</p>
          <Highlight className='JavaScript'>
            {selected && selected.feature && selected.feature.aliases ? JSON.stringify(selected.feature.aliases, null, 2) : null}
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