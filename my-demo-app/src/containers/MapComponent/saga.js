import { call, put, select, takeLatest, all, getContext  } from 'redux-saga/effects';
import queryString from 'query-string';
import { LOAD_LAYERS } from './constants';
import * as actions from './actions';
import LayersManager from "../../model/LayersManager";
import moment from "moment";
import layersHelper from "../../utils/layersHelper";

export function* getMap(options) {
  const gisAdapter = yield getContext('gisAdapter');

  try {
    const response = yield gisAdapter.getMap(options.mapId);
    if (response === null) {
      return;
    }
    const map = response.data;

    function processLayers(layers, parentId) {
      if (!parentId)
        parentId = '';

      return layers.map((layer) => {
        layer.path = parentId + (layer.id ? layer.id : layer.uid);
        layer.initialVisible = layer.visible;

        if (layer.mapLayerType === 'WMS') {
          layer.layers = queryString.parse(layer.layerUrl.toLocaleLowerCase()).layers;
        }

        if (layer.mapElements) {
          processLayers(layer.mapElements, layer.path + '/');
        }
        if (layer.groupElements) {
          processLayers(layer.groupElements, layer.path + '/');
        }

        if (layer.playback) {
          layer.playback.minValue = moment(layer.playback.minValue);
          layer.playback.maxValue = moment(layer.playback.maxValue);
          layer.playback.currentValue = layer.playback.minValue;
        }

        return layer;
      });
    }

    const mapElements = [];
    mapElements.push({
      path: 'layers',
      type: 'group',
      name: 'Тематические слои',
      description: 'Тематические слои',
      groupElements: processLayers(map.mapElements, 'layers'),
    });
    mapElements.push({
      path: 'baseLayers',
      type: 'group',
      name: 'Базовые слои',
      description: 'Базовые слои',
      groupElements: processLayers(map.baseMapElements, 'baseLayers'),
    });

    if (options.configuration.visible.indexOf('report') > -1) {
      mapElements.push({
        description: "Оформление отчета",
        initialVisible: true,
        mapLayerType: "reportLayer",
        name: "Оформление отчета",
        order: 999999,
        path: "reportLayer",
        type: "reportLayer",
        visible: true,
      });
    }

    if (options.extraLayers && options.extraLayers.length > 0) {
      const layers = layersHelper.toArray(mapElements).sort((a, b) => b.order - a.order).filter(x => x.type === 'layer');
      const extraLayers = options.extraLayers.sort((a, b) => a.order - b.order);

      mapElements.push({
        path: 'extraLayers',
        type: 'group',
        name: 'Дополнительные слои',
        description: 'Дополнительные слои',
        groupElements: extraLayers.map((x, i) => {
          const mapElement = {
            type: 'layer',
            mapLayerType: 'WMS',
            layerUrl: x.url,
            description: x.name,
            name: x.name,
            order: x.order,
            path: 'extraLayers/l' + i,
            visible: x.visible,
            initialVisible: x.visible,
          };

          layers.splice(x.order, 0, mapElement);
          return mapElement;
        }),
      });

      layers.map((x, i) => x.order = 999 - i);
    }

    const layers = (yield gisAdapter.getLayers()).data;
    const layersManager = new LayersManager();
    layersManager.init(layers);

    yield put(actions.layersLoaded(mapElements, map.refreshPeriod));
  } catch (err) {
    console.log(err);
    yield put(actions.layersLoadingError(err));
  }
}

export default function* saga() {
  yield takeLatest(LOAD_LAYERS, getMap);
}
