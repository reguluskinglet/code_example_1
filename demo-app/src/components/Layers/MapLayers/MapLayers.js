import * as React from 'react';
import PropTypes from 'prop-types';
import moment from 'moment';

import {withOl, OlProvider} from '../../../context';
import GisAdapterContext from '../../../adapters/GisAdapterContext';
import MapLayer from "./MapLayer";
import layersHelper from "../../../utils/layersHelper";

export class MapLayers extends React.Component {
  _layers = {};

  constructor(props) {
    super(props);
  }

  get layerContainer() {
    return this.props.ol.layerContainer || this.props.ol.map
  }

  handleLayers = (layers, gisAdapter) => {
    const makeLayers = (layers, layersAdded, layerContainer) => {
      for (let layer of layersHelper.toArray(layers)) {
        if (!layer.type || (layer.type !== 'base' && layer.type !== 'layer')) {
          continue;
        }

        if (layer.visible) {
          if (layersAdded.hasOwnProperty(layer.path)) {
            continue;
          }

          const mapLayer = new MapLayer(layerContainer, layer, gisAdapter);
          mapLayer.addToMap();
          layersAdded[layer.path] = mapLayer;

          continue;
        }

        if (layersAdded.hasOwnProperty(layer.path)) {
          const mapLayer = layersAdded[layer.path];
          mapLayer.removeFromMap().destroy();
          delete layersAdded[layer.path];
        }
      }
    };

    makeLayers(layers, this._layers, this.layerContainer);
  };

  updateLayers = () => {
    if (this.props.layers === false) {
      return;
    }

    layersHelper.toArray(this.props.layers).filter(x => x.refreshable).map(x => {
      const item = this._layers[x.path];
      if (item && item.layer) {
        const source = item.layer.getSource();
        source.setUrl(x.layerUrl + '&rnd=' + moment(new Date()).format('x'));
      }
    });
  };

  render() {
    return (
      <GisAdapterContext.Consumer>
        {
          gisAdapter => {
            if (this.props.layers !== false) {
              this.handleLayers(this.props.layers, gisAdapter);
            }
            return null;
          }
        }
      </GisAdapterContext.Consumer>
    )
  }

  componentWillUnmount() {
    this.layerContainer.removeLayer(this.olElement);

    clearInterval(this.timerId);
  }

  componentDidUpdate(prevProps, prevState, snapshot) {
    if (prevProps.refreshPeriod !== this.props.refreshPeriod && this.props.refreshPeriod > 0) {
      clearInterval(this.timerId);
      this.timerId = setInterval(this.updateLayers, this.props.refreshPeriod * 1000);
    }
  }
}

MapLayers.propTypes = {
  ol: PropTypes.shape({
    layerContainer: PropTypes.any,
    map: PropTypes.any,
  }),
  children: PropTypes.any,
  layers: PropTypes.oneOfType([
    PropTypes.array,
    PropTypes.bool,
  ]),
  refreshPeriod: PropTypes.any,
};

export default withOl(MapLayers);