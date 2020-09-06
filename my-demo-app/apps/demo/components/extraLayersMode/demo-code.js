const Code = `
  import * as React from 'react';
  
  // Импортируем картографический компонент
  import MapComponentApp from '../../../../src/';

  // Задаем URL backend'а ГИС-подсистемы
  const GIS_BASE_URL = 'http://api.gis.dev.sac.sphaera.cloud';
  // Задаем URL GeoServer'а
  const GIS_GEOSERVER_URL = 'http://127.0.0.1:8080/geoserver';
  // Задаем название параметра в localStorage с токеном
  const AUTH_TOKEN_LOCAL_STORAGE_KEY = 'AuthToken';

  // Задаем координаты центра карты
  const center = [15213650.715525128, 5747063.475428338];

  // Задаем уровень начального масштаба карты
  const zoom = 7;

  // Задаем ограничивающие параметры карты
  const limits = {
    // Устанавливаем максимально возможный зум карты
    maxZoom: 17,
    // Устанавливаем минимально возможный зум карты
    minZoom: 7
  };

  // Задаем идентификатор картографического проекта
  const mapId = 1007;
  // Задаем конфигурацию элементов карты
  const configuration = {
    visible: ['layers', 'scale'],
  };
  
  // Задаем дополнительные слои
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

  // Описываем react-компонент
  export const App = () => {
    return (
      // Оборачиваем компонент в div-элемент, полностью заполняющий родительский
      <div style={{width: '100%', height: '100%'}}>
        // Отображаем компонент, передавая ему определенные выше настройки
        <MapComponentApp
            viewMode='full'
            mapId={mapId}
            configuration={configuration}
            gisBaseUrl={GIS_BASE_URL} gisGeoServerUrl={GIS_GEOSERVER_URL} authTokenLocalStorageKey={AUTH_TOKEN_LOCAL_STORAGE_KEY}
            center={center} zoom={zoom} limits={limits}
            extraLayers={extraLayers}
          />
      </div>
    )
  };
`;

export default Code;