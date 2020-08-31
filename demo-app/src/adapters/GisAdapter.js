import request from '../utils/request';

class GisAdapter {
  setBaseUrl(baseUrl) {
    this._baseUrl = baseUrl;
  }
  setGeoServerUrl(geoServerUrl) {
    this._geoServerUrl = geoServerUrl;
  }
  setAuthTokenLocalStorageKey(authTokenLocalStorageKey) {
    this._authTokenLocalStorageKey = authTokenLocalStorageKey;
  }
  setOnError(onError) {
    this._onError = onError;
  }

  buildHeaders = () => {
    const token = localStorage[this._authTokenLocalStorageKey];
    if (!token || token.length === 0) {
      return {
      };
    }

    return {
      'auth-token': token,
    };
  };

  getBaseUrl = () => `${this._baseUrl}`;
  getGeoServerUrl = () => `${this._geoServerUrl}`;

  getMap = (mapId) => request(`${this._baseUrl}/sphaera/maps/${mapId}`, {mode: 'cors', headers: this.buildHeaders()}, this._onError);

  getLayers = () => request(`${this._baseUrl}/sphaera/layers/`, {mode: 'cors', headers: this.buildHeaders()}, this._onError);

  getMarkers = () => request(`${this._baseUrl}/sphaera/graphics/markers`, {mode: 'cors', headers: this.buildHeaders()}, this._onError);

  getMarkerUrl = (link) => `${this._baseUrl}/sphaera/util/files/${link}`;

  providerSearch = (type, value, params) => request(`${this._baseUrl}/sphaera/geocoder/search?geocoder=${type}&query=${value}&${params}`, {mode: 'cors', headers: this.buildHeaders()}, this._onError);

  getAliases = (layerId) => request(`${this._baseUrl}/sphaera/aliases?layerId=${layerId}`, {mode: 'cors', headers: this.buildHeaders()}, this._onError);
}

const gisAdapter = new GisAdapter();

export default gisAdapter;
