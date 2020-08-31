import {createSelector} from 'reselect';

const selectMain = (state) => state.get('main');

const makeSelectLayers = () => createSelector(
  selectMain,
  (mainState) => mainState.get('layers'),
);

const makeSelectRefreshPeriod = () => createSelector(
  selectMain,
  (mainState) => mainState.get('refreshPeriod'),
);

const makeSelectLoading = () => createSelector(
  selectMain,
  (mainState) => mainState.get('loading'),
);

const makeSelectError = () => createSelector(
  selectMain,
  (mainState) => mainState.get('error'),
);

export {
  selectMain,
  makeSelectLayers,
  makeSelectRefreshPeriod,
  makeSelectLoading,
  makeSelectError,
};
