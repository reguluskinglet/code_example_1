import 'babel-polyfill';
import React from 'react';
import ReactDOM from 'react-dom';
import GisMapApp from '../../src/index';

const MOUNT_NODE = document.getElementById('gisApp');

const GIS_BASE_URL = window.BACKEND_ADDR;
const center = [107933.8509731818, 96615.43737673666];
const zoom = 11;
const limits = {
  extent: [79135.12406543473, 71838.94052160066, 134295.26818255024, 120817.75426338427],
  maxZoom: 19,
  minZoom: 11
};

const render = () => {
  ReactDOM.render(
    <GisMapApp center={center} zoom={zoom} limits={limits} gisBaseUrl={GIS_BASE_URL}/>,
    MOUNT_NODE,
  );
};

if (module.hot) {
  module.hot.accept(['../../src/index'], () => {
    ReactDOM.unmountComponentAtNode(MOUNT_NODE);
    render();
  });
}

render();