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
            geoJson={geoJson} // Передаем GeoJSON для отрисовки объектов на карте
            onGeometryChanged={(geoJson) => {
              // Получаем GeoJSON измененных объектов на карте
            }}
          />
      </div>
    )
  };
`;

export default Code;