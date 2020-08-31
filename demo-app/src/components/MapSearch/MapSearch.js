import React from 'react';
import PropTypes from 'prop-types';

import './style.scss';
import debounce from "lodash/debounce";
import * as olProj from 'ol/proj';
import * as olExtent from "ol/extent";
import LayersManager from "../../model/LayersManager";
import IcoSearch from '../../icons/ico_search.svg';

export class MapSearch extends React.Component {
  state = {
    value: '',
    selected: null,
    providers: [],
  };

  constructor(props) {
    super(props);
    this.fetchId = 0;
    this.onSearch = debounce(this.onSearch, 800);
  }

  get map() {
    return window.OL;
  }

  serialize = (obj) => {
    const str = [];
    for (let p in obj)
      if (obj.hasOwnProperty(p)) {
        str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
      }
    return str.join("&");
  };
  getRandomId = () => {
    const min = Math.ceil(1000000);
    const max = Math.floor(9999999);
    return Math.floor(Math.random() * (max - min + 1)) + min;
  };

  onChange = (event) => {
    const value = event.target.value;
    this.setState({
      value,
    });

    this.onSearch();
  };

  onSearch = () => {
    const {
      value,
    } = this.state;
    const {
      configuration,
    } = this.props;

    this.props.layer.clear();
    this.fetchId += 1;

    const providers = [];
    if (value.length !== 0) {
      const match1 = value.match(/^([NSEWСЮВЗ-]?\d*)\.?(\d+[NSEWСЮВЗ-]?)/gi);
      const match2 = value.match(/([NSEWСЮВЗ-]?\d*)\.?(\d+[NSEWСЮВЗ-]?)$/gi);
      if (match1 && match1.length !== 0 && match2 && match2.length !== 0) {
        /*
        43, 131
        43.777044, 131.555554
        N43.777044, E131.555554
        43.777044N, 131.555554E
        С43.777044, В131.555554
        43.777044С, 131.555554В
        */

        const isLatitudeChar = (char) => {
          switch (char.toUpperCase()) {
            case 'N':
            case 'С':
            case 'S':
            case 'Ю': {
              return true;
            }
            case 'E':
            case 'В':
            case 'W':
            case 'З': {
              return false;
            }
          }
        };
        const isLongitudeChar = (char) => {
          switch (char.toUpperCase()) {
            case 'N':
            case 'С':
            case 'S':
            case 'Ю': {
              return false;
            }
            case 'E':
            case 'В':
            case 'W':
            case 'З': {
              return true;
            }
          }
        };

        const isLatitude  = (m) => isLatitudeChar(m[0][0]) || isLatitudeChar(m[0][m[0].length-1]);
        const isLongitude  = (m) => isLongitudeChar(m[0][0]) || isLongitudeChar(m[0][m[0].length-1]);

        const parseCoordinateValue = (m) => {
          switch (m[0][0].toUpperCase()) {
            case 'N':
            case 'С':
            case 'E':
            case 'В': {
              return parseFloat(m[0].substr(1));
            }
            case 'S':
            case 'Ю':
            case 'W':
            case 'З':  {
              return 0 - parseFloat(m[0].substr(1));
            }
          }

          switch (m[0][m[0].length-1].toUpperCase()) {
            case 'N':
            case 'С':
            case 'E':
            case 'В': {
              return parseFloat(m[0].substr(0, m[0].length - 1));
            }
            case 'S':
            case 'Ю':
            case 'W':
            case 'З': {
              return 0 - parseFloat(m[0].substr(0, m[0].length - 1));
            }
          }

          return parseFloat(m[0]);
        };

        const latitude = isLatitude(match2) ? parseCoordinateValue(match2) : parseCoordinateValue(match1);
        const longitude = isLongitude(match1) ? parseCoordinateValue(match1) : parseCoordinateValue(match2);

        const item = {
          id: this.getRandomId(),
          name: latitude + ', ' + longitude,
        };

        item.coordinates = [longitude, latitude];
        item.coordinates = olProj.transform(item.coordinates, 'EPSG:4326', 'EPSG:900913');
        this.props.layer.addFeature(item.id, item.coordinates, item);

        providers.push({
          type: 'coordinates',
          name: 'Координаты',
          results: [item],
          fetching: false,
        });

        this.setState({
          selected: item,
          providers,
        });

        // Вывод результатов поиска не должен позиционировать карту на первый найденный результат
        // this.onSelect(item, false);

        return;
      }

      for (let type in configuration) {
        providers.push({
          type,
          name: this.getProviderName(type),
          results: [],
          fetching: true,
        });
      }
    }

    this.setState({
      selected: null,
      providers,
    });

    providers.forEach((provider) => {
      this.onProviderSearch(this.fetchId, provider, value, configuration[provider.type]);
    });

    this.forceUpdate();
  };

  onReset = () => {
    this.reset();
  };

  onProviderSearch = (fetchId, provider, value, params) => {
    this.props.gisAdapter
      .providerSearch(provider.type, value, this.serialize(params))
      .then((response) => {
        if (fetchId !== this.fetchId) {
          return;
        }

        provider.fetching = false;
        console.debug(response.result);
        console.debug(response.status);

        if (response.status === 'OK') {
          const results = this.processProviderSearch(provider.type, response);
          results.forEach((item) => {
            if (item.coordinates) {
              item.coordinates = olProj.transform(item.coordinates, 'EPSG:4326', 'EPSG:900913');
              this.props.layer.addFeature(item.id, item.coordinates, item);
            }
            if (item.bbox) {
              item.bbox = olExtent.boundingExtent([[item.bbox[2], item.bbox[0]], [item.bbox[3], item.bbox[1]]]);
              item.bbox = olProj.transformExtent(item.bbox, 'EPSG:4326', 'EPSG:900913');
            }
          });

          if (results.length > 0) {
            const {
              selected,
            } = this.state;

            if (selected === null) {
              // Вывод результатов поиска не должен позиционировать карту на первый найденный результат
              // this.onSelect(results[0], true);
            }
          }

          provider.results = results;
        }

        this.forceUpdate();
      });
  };

  processProviderSearch = (providerType, response) => {
    switch (providerType) {
      case 'SPHAERA': {
        return [];
      }
      case 'DADATA':
      case 'YANDEX':{
        return response.data.formatted.map((item) => ({
          id: this.getRandomId(),
          name: item.address,
          bbox: item.bbox === null ? null : [parseFloat(item.bbox[0]), parseFloat(item.bbox[1]), parseFloat(item.bbox[2]), parseFloat(item.bbox[3])],
          coordinates: item.lon === null || item.lat === null ? null : [parseFloat(item.lon), parseFloat(item.lat)],
        }));
      }
      case 'LAYER': {
        return response.data.data.formatted.map((item) => ({
          id: this.getRandomId(),
          name: item.address.name,
          layerId: item.address.layerId,
          bbox: item.bbox === null ? null : [parseFloat(item.bbox[0]), parseFloat(item.bbox[1]), parseFloat(item.bbox[2]), parseFloat(item.bbox[3])],
          coordinates: item.lon === null || item.lat === null ? null : [parseFloat(item.lat), parseFloat(item.lon)],
        }));
      }
    }

    return [];
  };

  getProviderName = (providerType) => {
    switch (providerType) {
      case 'DADATA':
        return 'DaData';

      case 'YANDEX':
        return 'Яндекс';

      case 'LAYER':
        return 'Слои';
    }

    return '';
  };

  onSelect = (item, zoom) => {
    this.setState({
      selected: item,
    });

    if (item.bbox && item.bbox.length !== 0) {
      this.props.layer.fitExtent(item.bbox);
      this.props.layer.setSelectedFeature(item.id, true, zoom);
      return;
    }

    if (item.coordinates) {
      this.props.layer.center(item.coordinates);
      this.props.layer.setSelectedFeature(item.id, true, zoom);
      return;
    }

    this.props.layer.setSelectedFeature(null, false, zoom);
  };

  reset = () => {
    this.setState({
      value: '',
      selected: null,
      providers: [],
    });

    this.onSearch();
  };


  onMapSelect = (item) => {
    this.setState({
      selected: item,
    });

    if (item.bbox && item.bbox.length !== 0) {
      this.props.layer.setSelectedFeature(item.id, true);
      return;
    }

    if (item.coordinates) {
      this.props.layer.setSelectedFeature(item.id, true);
      return;
    }

    this.props.layer.setSelectedFeature(null, false);
  };


  componentDidUpdate(prevProps, prevState, snapshot) {
  };

  componentDidMount() {
  };

  componentWillUnmount() {
  };


  render() {
    const {
      value,
      providers,
    } = this.state;

    const hasResults = providers.length > 0;

    const className = 'gis-map-search' + (hasResults ? ' results' : '');

    return <div className={className}>
      <div className={'gis-map-search-input'}>
        <input type='text' value={value} placeholder='Поиск' onChange={this.onChange}/>
        {value && value.length > 0 ? <i className={'reset'} onClick={this.onReset}></i> : null}
        <i className={'search'} onClick={this.onSearch}><IcoSearch/></i>
      </div>
      <div className={'gis-map-search-panel gis-scrollbar' + (hasResults ? ' active' : '')}>
        <ul className={'gis-map-search-providers'}>
          {
            providers.map((provider) => this.renderProvider(provider))
          }
        </ul>
      </div>
    </div>;
  }

  renderProvider(provider) {
    if (provider.type === 'coordinates') {
      return <li
        className={'gis-map-search-provider-' + provider.type}
        key={'map-search-provider-' + provider.type}>
        {this.renderProviderContent(provider)}
      </li>;
    }

    if (provider.type === 'LAYER' && provider.results.length > 0) {
      const layers = [];
      const getLayer = (layerId) => {
        const arr = layers.filter(x => x.layerId === layerId);
        if (arr.length === 0) {
          layers.push({
            type: provider.type,
            layerId: layerId,
            layerName: (new LayersManager()).getLayerNameById(layerId),
            results: [],
          });

          return getLayer(layerId);
        }

        return arr[0];
      };

      provider.results.forEach(function(item){
        getLayer(item.layerId).results.push(item);
      });

      return (
        layers.map((layer) => <li
          key={'map-search-provider-' + provider.type + '-' + layer.layerId}>
          <div className={'gis-map-search-results-provider-name'}>{provider.name}: {layer.layerName}</div>
          {this.renderProviderContent(layer)}
        </li>)
      );
    }

    return <li
      key={'map-search-provider-' + provider.type}>
      <div className={'gis-map-search-results-provider-name'}>{provider.name}</div>
      {this.renderProviderContent(provider)}
    </li>;
  }

  renderProviderContent(provider) {
    if (provider.fetching) {
      return <div className={'gis-map-search-processing'}>поиск...</div>;
    }

    if (provider.results.length === 0) {
      return <div className={'gis-map-search-empty'}>не найдено</div>;
    }

    return <ul className={'gis-map-search-results'}>
      {
        provider.results.map((item) => this.renderResultItem(provider, item))
      }
    </ul>;
  }

  renderResultItem(provider, item) {
    const {
      selected,
    } = this.state;

    return <li
      key={'map-search-provider-' + provider.type + '-' + item.id}
      onClick={() => { this.onSelect(item, true); }}
      className={selected != null && selected.id === item.id ? 'selected' : ''}>
      {item.name}
    </li>;
  }
}

MapSearch.propTypes = {
  gisAdapter: PropTypes.object,
  layer: PropTypes.any,
  configuration: PropTypes.any,
};

export default MapSearch;