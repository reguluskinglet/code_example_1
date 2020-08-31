import {fromJS} from 'immutable';
import * as constants from './constants';

const initialState = fromJS({
  loading: false,
  error: false,
  layers: false,
  refreshPeriod: null,
});

function mainReducer(state = initialState, action) {
  switch (action.type) {
    case constants.LOAD_LAYERS:
      return state
        .set('loading', true)
        .set('error', false)
        .set('layers', false);
    case constants.LOAD_LAYERS_SUCCESS:
      return state
        .set('layers', action.layers)
        .set('refreshPeriod', action.refreshPeriod)
        .set('loading', false);
    case constants.LOAD_LAYERS_ERROR:
      return state
        .set('error', action.error)
        .set('loading', false)
        .set('layers', false);

    case constants.CHANGE_LAYERS: {
      return state.set('layers', action.layers);
    }

    default:
      return state;
  }
}

export default mainReducer;
