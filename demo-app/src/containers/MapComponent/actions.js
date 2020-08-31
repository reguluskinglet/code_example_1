import {
  CHANGE_LAYERS,
  LOAD_LAYERS,
  LOAD_LAYERS_SUCCESS,
  LOAD_LAYERS_ERROR,
} from './constants';

export function loadLayers(mapId, configuration, extraLayers) {
  return {
    type: LOAD_LAYERS,
    mapId,
    configuration,
    extraLayers,
  };
}

export function layersLoaded(layers, refreshPeriod) {
  return {
    type: LOAD_LAYERS_SUCCESS,
    layers,
    refreshPeriod,
  };
}

export function layersLoadingError(error) {
  return {
    type: LOAD_LAYERS_ERROR,
    error,
  };
}

export function changeLayers(layers) {
  return {
    type: CHANGE_LAYERS,
    layers,
  };
}