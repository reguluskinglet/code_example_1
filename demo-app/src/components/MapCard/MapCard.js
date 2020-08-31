import * as React from 'react';
import PropTypes from 'prop-types';

import {withOl} from '../../context';
import Overlay from 'ol/Overlay';

import './style.scss';

export class MapCard extends React.Component {
  state = {
    feature: null,
    aliases: null,
    contentWidth: 0,
    contentHeight: 0,
  };

  overlay = null;

  get map() {
    return this.props.ol.map;
  }

  constructor(props) {
    super(props);

    this.contentRef = React.createRef();
    setInterval(() => {
      if (this.contentRef.current) {
        const {contentWidth, contentHeight} = this.state;

        const offsetWidth = this.contentRef.current.offsetWidth;
        const offsetHeight = this.contentRef.current.offsetHeight;
        if (offsetWidth !== contentWidth || offsetHeight !== contentHeight) {
          this.setState({
            contentWidth: offsetWidth,
            contentHeight: offsetHeight,
          });
        }
      }
    }, 100);
  }


  onMapPointerMove = (evt) => {
    const hit = false;
    this.map.getTarget().style.cursor = hit ? 'pointer' : '';
  };

  onMapMoveStart = (evt) => {
    this.map.un('pointermove', this.onMapPointerMove);
  };

  onMapMoveEnd = (evt) => {
    this.map.on('pointermove', this.onMapPointerMove);
  };

  convertToClick = (e) => {
    const evt = new MouseEvent('click', { bubbles: true });
    evt.stopPropagation = () => {};
    e.target.dispatchEvent(evt)
  };


  reset = () => {
    this.setFeature(null, null);
  };

  setFeature = (coordinate, feature) => {
    this.setState({
      feature,
      aliases: null,
    });

    if (this.overlay) {
      this.map.removeOverlay(this.overlay);
    }

    if (!feature) {
      return;
    }

    const reportStyle = feature.get ? feature.get('style') : null;
    if (reportStyle) {
      if (reportStyle.marker.description.length === 0) {
        return;
      }
    } else if (feature.layerId) {
      this.props.gisAdapter
        .getAliases(feature.layerId)
        .then((response) => {
          this.setState({
            aliases: response.data,
          });
        });
    }

    this.overlay = new Overlay({
      element: this.popupContainer,
      autoPan: true,
      autoPanAnimation: {
        duration: 250,
      },
      position: coordinate,
    });

    this.map.addOverlay(this.overlay);

    const s = () => {
      const clientWidth = this.map.getViewport().clientWidth;
      const pixel = this.map.getPixelFromCoordinate(coordinate);
      const centerPixel = this.map.getPixelFromCoordinate(this.map.getView().getCenter());

      const cardSize = this.getActualCardSize();
      if (cardSize.height === undefined || cardSize.height === 0 || cardSize.width === undefined || cardSize.width === 0) {
        setTimeout(() => s(), 100);
        return;
      }

      if (cardSize.width > 0 && cardSize.height > 0) {
        if (pixel[0] + (cardSize.width / 2) > clientWidth) {
          centerPixel[0] = centerPixel[0] + (pixel[0] + (cardSize.width / 2) - clientWidth) + 15;
        } else if (pixel[0] < (cardSize.width / 2)) {
          centerPixel[0] = centerPixel[0] - ((cardSize.width / 2) - pixel[0]) - 15;
        }
        if (pixel[1] < (cardSize.height)) {
          centerPixel[1] = centerPixel[1] - ((cardSize.height) - pixel[1]) - 15;
        }
      }

      this.map.getView().animate({
        center: this.map.getCoordinateFromPixel(centerPixel),
        duration: 500,
      });
    };

    s();
  };

  getCardSize = () => {
    const {width, height} = this.props;

    return {
      maxWidth: width === null || width === undefined ? 350 : width,
      maxHeight: height === null || height === undefined ? 200 : height,
    };
  };
  getActualCardSize = () => {
    const {contentWidth, contentHeight} = this.state;
    const cardSize = this.getCardSize();

    return {
      width: contentWidth < cardSize.maxWidth ? contentWidth : cardSize.maxWidth,
      height: (contentHeight < cardSize.maxHeight ? contentHeight : cardSize.maxHeight) + 28 + 16,
    };
  };


  componentDidMount() {
    const map = this.map;

    this.popupContainer.onmouseenter = (evt) => {
      const tooltip = map.getOverlayById('tooltip');
      if (tooltip) {
        tooltip.setPosition(undefined);
        tooltip.set('suppress', true);
      }
      map.getTarget().style.cursor = '';
    };
    this.popupContainer.onmouseleave = (evt) => {
      const tooltip = map.getOverlayById('tooltip');
      if (tooltip) {
        tooltip.setPosition(undefined);
        tooltip.set('suppress', false);
      }
      map.getTarget().style.cursor = '';
    };
    this.popupContainer.oncontextmenu = (evt) => {
      evt.stopImmediatePropagation();
    };

    map.on('pointermove', this.onMapPointerMove);
    map.on('movestart', this.onMapMoveStart);
    map.on('moveend', this.onMapMoveEnd);
  }

  componentDidUpdate() {
    if (this.popupCloser) {
      this.popupCloser.onclick = () => {
        this.overlay.setPosition(undefined);
        this.popupCloser.blur();
        this.featureId = null;
        return false;
      };
    }
  }

  componentWillUnmount() {
    const map = this.map;

    if (this.overlay) {
      map.removeOverlay(this.overlay);
      this.overlay = null;
    }

    map.un('pointermove', this.onMapPointerMove);
    map.un('movestart', this.onMapMoveStart);
    map.un('moveend', this.onMapMoveEnd);
  }

  render() {
    const {feature, aliases, contentWidth, contentHeight} = this.state;

    const cardSize = this.getCardSize();

    const mapCardStyle = {
    };

    if (contentWidth > 0 && contentHeight > 0) {
      const cardSize = this.getActualCardSize();
      mapCardStyle.marginLeft = '-' + cardSize.width/2 + 'px';
      mapCardStyle.marginTop = '-' + cardSize.height + 'px';
      mapCardStyle.visibility = 'visible';
    }

    return <div
      id="map-card"
      className="ol-popup"
      ref={(el) => this.popupContainer = el}
      style={mapCardStyle}
    >
      <a
        href="javascript:void(0)"
        id="popup-closer"
        className="ol-popup-closer"
        ref={(el) => this.popupCloser = el}/>
      <div className={'header'} style={{maxWidth: cardSize.maxWidth}}>
        {this.renderHeader(feature, aliases)}
      </div>
      <div id="popup-content" className={'content gis-scrollbar'} style={cardSize}>
        <div ref={this.contentRef} className={'content-wrapper'}>
          {this.renderContent(feature, aliases)}
        </div>
      </div>
    </div>;
  }

  renderHeader = (feature, aliases) => {
    if (!feature) {
      return '';
    }

    const {headerRender} = this.props;

    if (headerRender) {
      const header = headerRender(feature, aliases);
      if (header) {
        return header;
      }
    }

    return 'Атрибуты объекта';
  };

  renderContent = (feature, aliases) => {
    if (!feature) {
      return '';
    }

    const {contentRender} = this.props;

    if (contentRender && feature) {
      const content = contentRender(feature, aliases);
      if (content) {
        return content;
      }
    }

    return this.renderFeature(feature, aliases);
  };

  renderFeature(feature, aliases) {
    if (!feature) {
      return null;
    }

    const reportStyle = feature.get ? feature.get('style') : null;
    if (reportStyle) {
      return <div className={'report-card'}>{reportStyle.marker.description}</div>
    }

    const arr = [];
    for (let propertyName in feature.properties) {
      if (propertyName === 'bbox' || propertyName.startsWith('gs_pointstacker_')) {
        continue;
      }

      const alias = aliases ? aliases.find(x => x.field === propertyName) : null;
      const title = alias ? alias.alias : propertyName;
      const value = feature.properties[propertyName];

      if (typeof value === 'object') {
        continue;
      }

      /*if (value.length === 0) {
        continue;
      }*/

      arr.push({title, value});
    }

    return <div>
      {arr.map((x, i) => (<div key={i}>
        <div className={'title'}>{x.title}:</div>
        <div className={'value'}>{x.value}</div>
      </div>))}
    </div>;

    return <div>
      {aliases.map(x => (<div key={x.id}>
        <div className={'title'}>{x.alias}:</div>
        <div className={'value'}>{feature[x.field]}</div>
      </div>))}
    </div>;
  };
}

MapCard.propTypes = {
  gisAdapter: PropTypes.object,
  ol: PropTypes.shape({
    map: PropTypes.any,
  }),
  headerRender: PropTypes.func,
  contentRender: PropTypes.func,
  width: PropTypes.number,
  height: PropTypes.number,
};

export default withOl(MapCard);