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

  // Описываем react-компонент
  export const App = () => {
    return (
      // Оборачиваем компонент в div-элемент с фиксированными размерами
      <div style={{width: 470, height: 480}}>
        // Отображаем компонент, передавая ему определенные выше настройки
        <MapComponentApp
            viewMode='fixed'
            mapId={mapId}
            configuration={configuration}
            gisBaseUrl={GIS_BASE_URL} gisGeoServerUrl={GIS_GEOSERVER_URL} authTokenLocalStorageKey={AUTH_TOKEN_LOCAL_STORAGE_KEY}
            center={center} zoom={zoom} limits={limits}
          />
      </div>
    )
  };
`;

export default Code;