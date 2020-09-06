export default class featuresHelper {
  static getTooltipName (featureProperties, layerId, defaultName) {
    return '';
  }

  static isPolygonFeature(feature) {
    return this.isPolygonGeometry(feature.getGeometry());
  };
  static isPolygonGeometry(geometry) {
    return geometry.getType() === 'Polygon' || geometry.getType() === 'MultiPolygon';
  };

  static getSmallerFeature(features) {
    const points = features.filter((x) => x.geometryType === 'Point');
    if (points.length !== 0) {
      return points[0];
    }

    const lines = features.filter((x) => x.geometryType === 'LineString' || x.geometryType === 'MultiLineString');
    if (lines.length !== 0) {
      return lines[0];
    }

    const polygons = features
      .filter((x) => x.geometryType === 'Polygon' || x.geometryType === 'MultiPolygon')
      .sort((a, b) => (a.geometry.getArea() > b.geometry.getArea()) ? 1 : ((b.geometry.getArea() > a.geometry.getArea()) ? -1 : 0));
    if (polygons.length !== 0) {
      return polygons[0];
    }

    return null;
  };
};