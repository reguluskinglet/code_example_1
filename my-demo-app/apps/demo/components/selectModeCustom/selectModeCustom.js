import * as React from 'react';
import ReactDOM from 'react-dom';
import {Button, Tabs} from 'antd';
import { Menu, Card, Icon, message } from 'antd';

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

export class SelectModeCustom extends React.Component<any, any> {
  state = {
    selected: null,
    customCardContent: '',
    cardWidth: 300,
    cardHeight: 300,
  };

  constructor(props) {
    super(props);
    this.contentRef = React.createRef();
    this.cardWidthRef = React.createRef();
    this.cardHeightRef = React.createRef();
  }

  render() {
    const {selected, customCardContent, cardWidth, cardHeight} = this.state;

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

    const onClick = ({ key }) => {
      message.info(`${key}`);
      document.getElementById('my-menu').style.display='none';
    };

    const my_handler = (click, feature) => {
      this.setState({selected: {
        click,
        feature
      }});

      document.getElementById('my-menu').style.display = 'none';
      if(click && click.pointerEvent && click.pointerEvent.button != 0) {
        // show menu
        if (feature === undefined || feature === null ||feature.layerId === undefined || feature.layerId === null)
          return;

        const menuItems = [];
        menuItems.push(<Menu.Item key="common">Общее меню</Menu.Item>);
        if (feature.layerId == 1012) {
          menuItems.push(<Menu.Item key="road">Отремонтировать дорогу</Menu.Item>);
        }
        if (feature.layerId == 1013) {
          menuItems.push(<Menu.Item key="river">Проверить ГИМС</Menu.Item>);
        }
        if (feature.layerId == 1008) {
          menuItems.push(<Menu.Item key="town">Увеличить пенсии в городе</Menu.Item>);
        }
        ReactDOM.render(
          <div style={{position: 'fixed', top: `${click.pointerEvent.clientY}px`, left: `${click.pointerEvent.clientX}px`}}>
            <Menu style={{display: 'block'}} onClick={onClick} selectedKeys={[]}>{menuItems}</Menu>
          </div>,
          document.getElementById('my-menu')
        );
        document.getElementById('my-menu').style.display='block';
      }

      console.log(click);
      console.log(feature);
    };

    const onExtentChanged = ({zoom, extent, center}) => {
      console.log({zoom, extent, center});
      document.getElementById('my-menu').style.display = 'none';
    };

    const cardHeaderRender = (feature, aliases) => 'Мой заголовок';
    const cardContentRender = (feature, aliases) => {
      if (customCardContent && customCardContent.length > 0) {
        return <div dangerouslySetInnerHTML={{__html: customCardContent}}></div>
      }

      return <div>
        <p>Произвольный контент</p>
        <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/d/de/Flag_of_Vladivostok%2C_Russia.png/128px-Flag_of_Vladivostok%2C_Russia.png" />
        <p style={{whiteSpace: 'pre-wrap'}}>{JSON.stringify(feature.properties, null, 2)}</p>
      </div>;
    };

    const onSetCustomContent = () => {
      this.setState({
        customCardContent: this.contentRef.current.value,
        cardWidth: parseInt(this.cardWidthRef.current.value),
        cardHeight: parseInt(this.cardHeightRef.current.value),
      })
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
              onSelect={my_handler}
              onExtentChanged={onExtentChanged}
              cardHeaderRender={cardHeaderRender}
              cardContentRender={cardContentRender}
              cardWidth={cardWidth}
              cardHeight={cardHeight}
              mapDragPanKinetic={{
                decay: -0.005,
                minVelocity: 2,
                delay: 1000
              }}
              clusterClickMode={'none'}
            />
            <div id='my-menu' />
          </div>
        </TabPane>
        <TabPane tab="Интеграция" key="2" style={{overflowY: 'scroll', paddingRight: '10px'}}>
          <p>Произвольный контент инфотула:</p>
          <p><textarea ref={this.contentRef} style={{width: '100%'}} rows='3' /></p>
          <p>Максимальная ширина инфотула:</p>
          <p><input defaultValue={cardWidth} ref={this.cardWidthRef} type={'text'} style={{width: '100%'}} /></p>
          <p>Максимальная высота инфотула:</p>
          <p><input defaultValue={cardHeight} ref={this.cardHeightRef} type={'text'} style={{width: '100%'}} /></p>
          <p><button onClick={onSetCustomContent}>Установить</button></p>
          <hr/>

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
            {selected && selected.click && selected.click.pointerEvent ? (selected.click.pointerEvent.x + ', ' + selected.click.pointerEvent.y) : ''}
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

          <p>Код React-компонента:</p>
          <Highlight className='JavaScript'>
            {Code}
          </Highlight>
        </TabPane>
      </Tabs>
    );
  }
}