import { connect } from 'react-redux';
import { compose } from 'redux';
import { createStructuredSelector } from 'reselect';
import injectReducer from '../../utils/injectReducer';
import injectSaga from '../../utils/injectSaga';
import * as selectors from './selectors';
import { changeLayers, loadLayers } from './actions';
import reducer from './reducer';
import saga from './saga';
import MapComponent from './MapComponent';

const mapDispatchToProps = (dispatch, ownProps) => ({
  onChangeLayers: (layers) => dispatch(changeLayers(layers)),
  onComponentDidMount: () => dispatch(loadLayers(ownProps.mapId, ownProps.configuration, ownProps.extraLayers)),
});

const mapStateToProps = createStructuredSelector({
  layers: selectors.makeSelectLayers(),
  refreshPeriod: selectors.makeSelectRefreshPeriod(),
  loading: selectors.makeSelectLoading(),
  error: selectors.makeSelectError(),
});

const withConnect = connect(mapStateToProps, mapDispatchToProps);
const withReducer = injectReducer({ key: 'main', reducer });
const withSaga = injectSaga({ key: 'main', saga });

export default compose(withReducer, withSaga, withConnect)(MapComponent);
export { mapDispatchToProps };
