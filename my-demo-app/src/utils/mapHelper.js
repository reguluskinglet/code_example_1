export default class mapHelper {
  static centerOn(coordinates) {
    const map = window.OL;
    const mapSize = map.getSize();
    const mapView = map.getView();
    mapView.centerOn(coordinates, mapSize, [mapSize[0]/2, mapSize[1]/2]);
    mapView.setZoom(mapView.getMaxZoom() - 2);
  }

  static fitExtent(extent, duration) {
    if (!extent || extent.length === 0) {
      return;
    }

    if (!duration) {
      duration = 0;
    }

    const map = window.OL;
    map.getView().fit(extent, {
      size: map.getSize(),
      duration: duration,
    });
  }
};