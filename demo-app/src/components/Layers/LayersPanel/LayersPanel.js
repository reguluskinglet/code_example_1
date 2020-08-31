import React from 'react';
import PropTypes from 'prop-types';
import CheckboxTree from 'react-checkbox-tree';
import './style.scss';
import 'react-checkbox-tree/lib/react-checkbox-tree.css';
import layersHelper from "../../../utils/layersHelper";
import IcoResetLayers from '../../../icons/ico_reset_layers.svg';

export default class LayersPanel extends React.PureComponent { // eslint-disable-line react/prefer-stateless-function
  state = {
    expanded: ['layers', 'baseLayers', 'extraLayers'],
  };

  constructor(props) {
    super(props);
  }


  componentDidUpdate(prevProps, prevState, snapshot) {
  }

  buildTreeNodes(items) {
    if (items === false)
      return [];

    return items.map((item) => {
      const treeNode = {
        value: item.path,
        label: item.name,
        title: item.description,
        className: this.isActive(item) ? 'active' : '',
        source: item,
      };

      if (item.path === 'layers') {
        treeNode.className = 'layers';
      }
      if (item.path === 'baseLayers') {
        treeNode.className = 'baseLayers';
      }
      if (item.path === 'reportLayer') {
        treeNode.className = 'reportLayer';
      }
      if (item.path === 'extraLayers') {
        treeNode.className = 'extraLayers';
      }

      if (item.mapElements) {
        treeNode.children = this.buildTreeNodes(item.mapElements);
      }
      if (item.groupElements) {
        treeNode.children = this.buildTreeNodes(item.groupElements);
      }

      return treeNode;
    });
  };

  isActive(item) {
    if (item.type === 'layer') {
      return item.visible;
    }

    if (item.type === 'group') {
      return item.groupElements.filter(x => this.isActive(x)).length > 0;
    }

    return false;
  };

  getVisible(layers) {
    if (layers === false)
      return [];

    return layersHelper.toArray(layers).filter((item) => item.visible).map((item) => item.path );
  };
  setVisible(layers, ids, targetNode) {
    const layersArray = layersHelper.toArray(layers);

    if (targetNode.checked) {
      const targetLayer = layersArray.filter(x => x.path === targetNode.value)[0];
      if (targetLayer.type === 'base') {
        layersArray.filter(x => x.path !== targetLayer.path && x.type === 'base').forEach(x => {
          const index = ids.indexOf(x.path);
          if (index !== -1) {
            ids.splice(index, 1);
          }
        });
      }
    }

    layersArray.map((item) => {
      item.visible = ids.indexOf(item.path) > -1;
    });
  }

  resetLayers = () => {
    const {layers} = this.props;

    layersHelper.toArray(layers).map(layer => layer.visible = layer.initialVisible);
    this.props.onLayersChanged(layers);
  };

  render() {
    const {visible} = this.props;
    const {expanded} = this.state;
    const className = visible ? 'layers-panel' : 'layers-panel off';

    const nodes = this.buildTreeNodes(this.props.layers);
    const checked = this.getVisible(this.props.layers);

    return (
      <div className={className}>
        <div className={'header'}>
          Слои
          <div className={'resetLayersBtn'} onClick={this.resetLayers}><IcoResetLayers /></div>
          <span className={'close'} onClick={() => this.props.onClose() }></span>
        </div>
        <div className={'content gis-scrollbar'}>
          <CheckboxTree
            nodes={nodes}
            checked={checked}
            expanded={expanded}
            onCheck={(ids, targetNode) => {
              if (targetNode.value === 'baseLayers') {
                return;
              }

              this.setVisible(this.props.layers, ids, targetNode);
              this.props.onLayersChanged(this.props.layers);
            }}
            onExpand={ids => this.setState({expanded: ids})}
            showNodeIcon={false}
            icons={{
              check: <span className="rct-icon rct-icon-check" />,
              uncheck: <span className="rct-icon rct-icon-uncheck" />,
              halfCheck: <span className="rct-icon rct-icon-half-check" />,
              expandClose: <span className="rct-icon rct-icon-expand-close" />,
              expandOpen: <span className="rct-icon rct-icon-expand-open" />,
              expandAll: <span className="rct-icon rct-icon-expand-all" />,
              collapseAll: <span className="rct-icon rct-icon-collapse-all" />,
              parentClose: <span className="rct-icon rct-icon-parent-close" />,
              parentOpen: <span className="rct-icon rct-icon-parent-open" />,
              leaf: <span className="rct-icon rct-icon-leaf" />,
            }}
          />
        </div>
      </div>
    );
  }
}

LayersPanel.propTypes = {
  visible: PropTypes.bool,
  layers:  PropTypes.any,
  onLayersChanged: PropTypes.func,
  onClose: PropTypes.func,
};
