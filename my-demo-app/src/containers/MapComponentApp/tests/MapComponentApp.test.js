import React from 'react';
import ReactDOM from 'react-dom';
import MapComponentApp from '../MapComponentApp';

it('renders without crashing', () => {
  const div = document.createElement('div');
  ReactDOM.render(<MapComponentApp />, div);
  ReactDOM.unmountComponentAtNode(div);
});
