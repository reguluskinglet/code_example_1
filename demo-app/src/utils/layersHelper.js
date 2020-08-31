export default class layersHelper {
  static toArray(layers, items) {
    if (layers === false)
      return [];

    if (!items) {
      items = [];
    }

    layers.map((layer) => {
      items.push(layer);

      if (layer.mapElements) {
        this.toArray(layer.mapElements, items);
      }
      if (layer.groupElements) {
        this.toArray(layer.groupElements, items);
      }
    });

    return items;
  };
};