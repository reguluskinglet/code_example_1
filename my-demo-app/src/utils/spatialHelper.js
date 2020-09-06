import * as turf from "@turf/turf/index";

export default class spatialHelper {
  static intersect (geometry) {
    switch (geometry.getType()) {
      case 'LineString': {
        return this.intersectLines(geometry);
      }
      case 'Polygon': {
        for (let linearRing of geometry.getLinearRings()) {
          const result = this.intersectLines(linearRing, {
            ignoreVertex: true,
            ignorePointOnLine: true,
          });
          if (result) {
            return true;
          }
        }
        break;
      }
    }

    return false;
  }

  static intersectLines (geometry, options) {
    if (!options) {
      options = {
        ignoreVertex: false,
        ignorePointOnLine: false,
      };
    }

    const coordinates = geometry.getCoordinates();

    const lines = [];
    const mercatorLines = [];
    for (let i = 0; i < coordinates.length - 1; i++) {
      lines.push(turf.lineString([turf.toWgs84(coordinates[i]), turf.toWgs84(coordinates[i + 1])]));
      mercatorLines.push(turf.lineString([coordinates[i], coordinates[i + 1]]))
    }

    const isLinePoint = (point, line) => {
      for (let c of line.geometry.coordinates) {
        if (c[0] === point.geometry.coordinates[0] && c[1] === point.geometry.coordinates[1]) {
          return true;
        }
      }

      return false;
    };

    for (let line1 of lines) {
      for (let line2 of lines) {
        const intersects = turf.lineIntersect(line1, line2);
        for (let feature of intersects.features) {
          if (isLinePoint(feature, line1) || isLinePoint(feature, line2)) {
            continue;
          }
          return true;
        }
      }
    }

    for (let coordinate of coordinates) {
      if (!options.ignoreVertex && coordinates.filter((x) => this.isEqualsCoordinates(x, coordinate)).length > 1) {
        return true;
      }
      for (let line of mercatorLines) {
        if (!options.ignorePointOnLine && line.geometry.coordinates.filter((x) => x === coordinate).length === 0 && this.isPointOnLine(coordinate, line.geometry.coordinates)) {
          return true;
        }
      }
    }

    return false;
  }

  static isPointOnLine(pointCoordinates, lineCoordinates) {
    const pt = turf.toWgs84(turf.point(pointCoordinates));
    const line = turf.toWgs84(turf.lineString(lineCoordinates));
    const snapped = turf.nearestPointOnLine(line, pt);
    return snapped.properties.dist < 0.00001;
  }

  static isEqualsCoordinates(first, second) {
    if (!first || !second) {
      return false;
    }

    return first[0] === second[0] && first[1] === second[1];
  }

  static isEqualGeometry(geometry1, geometry2) {
    if (geometry1 && !geometry2 || !geometry1 && geometry2) {
      return false;
    }
    if (!geometry1 && !geometry2) {
      return true;
    }

    return turf.booleanEqual({
      "type": geometry1.getType(),
      "coordinates": geometry1.getCoordinates(),
    }, {
      "type": geometry2.getType(),
      "coordinates": geometry2.getCoordinates(),
    });
  }

  static isContains(first, second) {
    if (!first || !second) {
      return false;
    }

    return turf.booleanContains({
      "type": first.getType(),
      "coordinates": first.getCoordinates(),
    }, {
      "type": second.getType(),
      "coordinates": second.getCoordinates(),
    });
  }

  static getLineSegments (geometry) {
    const lines = [];
    
    if (geometry.getType() !== 'LineString') {
      return lines;
    }

    const coordinates = geometry.getCoordinates();
    for (let i = 0; i < coordinates.length - 1; i++) {
      const segment = turf.lineString([coordinates[i], coordinates[i + 1]]);
      segment.properties.center = turf.center(segment);
      lines.push(segment);
    }

    return lines;
  }

  static nearestPoint(coordinate, points) {
    if (points.length === 0) {
      return null;
    }

    const targetPoint = turf.point(turf.toWgs84(coordinate));
    const featureCollection = turf.featureCollection(points.reverse().map(x => turf.point(turf.toWgs84(x.geometry.getCoordinates()))));
    const nearest = turf.nearestPoint(targetPoint, featureCollection);

    return points[nearest.properties.featureIndex];
  }
};