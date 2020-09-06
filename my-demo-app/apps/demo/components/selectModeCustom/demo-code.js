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
    visible: ['search', 'layers', 'scale', 'select'],
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
            onSelect={(click, feature) => {
                // Обработка события "Нажатие на карте"
            }}
            onExtentChanged={({zoom, extent, center}) => {
                // Обработка события "Экстент карты изменен"
            }
            cardHeaderRender={(feature, aliases) => {
                // Формирование заголовка
                return 'Заголовок';
            }}
            cardContentRender={(feature, aliases) => {
                // Формирование содержимого
                return <p>{feature.properties.name}</p>;
            }}
            cardWidth={300} // Ширина инфотула
            cardHeight={300} // Высота инфотула
            mapDragPanKinetic={{ // Параметр инерции скрола карты, описание https://openlayers.org/en/latest/apidoc/module-ol_Kinetic-Kinetic.html
              decay: -0.005,
              minVelocity: 2,
              delay: 1000
            }}
            clusterClickMode={'none'} // Не приближаться при клике по кластеру
          />
      </div>
    )
  };
`;

export default Code;