let instance = null;

export default class LayersManager {
  constructor() {
    if (!instance) {
      instance = this;
    }
    return instance;
  }

  init(layers) {
    this._layers = layers;
  }

  getLayers() {
    return this._layers;
  }
  getLayerById(layerId) {
    const filtered = this._layers.filter((layer) => layer.id === layerId);
    if (filtered.length > 0) {
      return filtered[0];
    }

    return null;
  }

  getLayerNameById(layerId) {
    const layer = this.getLayerById(layerId);
    return layer === null ? '' : layer.name;
  }
}