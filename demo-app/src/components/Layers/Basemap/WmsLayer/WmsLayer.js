import * as React from 'react';
import PropTypes from 'prop-types';

import olImage from 'ol/layer/image';
import olSourceImageWms from 'ol/source/imagewms';

import {withOl, OlProvider} from "../../../../context";

export class WmsLayer extends React.Component {
  olElement = null;

  constructor(props) {
    super(props);
    this.olElement = this.createOlElement(props);
  }

  createOlElement(props) {
    const source = new olSourceImageWms({
      url: props.url,
      ratio: 1,
      serverType: 'geoserver'
    });

    return new olImage({source});
  }

  get layerContainer() {
    return this.props.ol.layerContainer || this.props.ol.map
  }

  render() {
    const {children} = this.props;
    if (children == null) {
      return null
    }
    return this.contextValue == null ? (
      <React.Fragment>{children}</React.Fragment>
    ) : (
      <OlProvider value={this.contextValue}>{children}</OlProvider>
    )
  }

  componentDidMount() {
    if (this.props.zIndex) {
      this.olElement.setZIndex(this.props.zIndex);
    }
    this.layerContainer.addLayer(this.olElement);
  }

  componentWillUnmount() {
    this.layerContainer.removeLayer(this.olElement);
  }
}

WmsLayer.propTypes = {
  ol: PropTypes.shape({
    layerContainer: PropTypes.any,
    map: PropTypes.any
  }),
  children: PropTypes.any,
  zIndex: PropTypes.number,
  url: PropTypes.any,
};

export default withOl(WmsLayer);