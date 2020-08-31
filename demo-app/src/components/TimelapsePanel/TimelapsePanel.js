import React from 'react';
import PropTypes from 'prop-types';
import Slider from 'rc-slider';
import moment from "moment";
import debounce from "lodash/debounce";
import DatePicker, { registerLocale } from "react-datepicker";
import ru from 'date-fns/locale/ru';
import IcoTimeSpeed from '../../icons/ico_time_speed.svg'
import IcoFromTheStart from '../../icons/ico_from_the_start.svg'
import IcoBackward from '../../icons/ico_backward.svg'
import IcoPlay from '../../icons/ico_play.svg'
import IcoStop from '../../icons/ico_stop.svg'
import IcoForward from '../../icons/ico_forward.svg'
import IcoToTheEnd from '../../icons/ico_to_the_end.svg'
import IcoSettings from '../../icons/ico_settings.svg'
import IcoSpeedMinus from '../../icons/ico_speed_minus.svg'
import IcoSpeedPlus from '../../icons/ico_speed_plus.svg'
import ReactCalendarInput from './ReactCalendarInput';
import Select from 'react-select'

import './style.scss';
import 'rc-slider/assets/index.css';
import 'react-datepicker/dist/react-datepicker.css';
import layersHelper from "../../utils/layersHelper";

registerLocale('ru', ru);

export default class TimelapsePanel extends React.PureComponent { // eslint-disable-line react/prefer-stateless-function
  state = {
    play: false,
    minValue: null,
    maxValue: null,
    currentValue: null,
    direction: 1,
    tickIntervalIndex: 2,
    tickValue: 1,
    tickType: 'minutes',
    settingsPanel: false,
    settingsPanel2: false,
    settingsMinDate: null,
    settingsMinTime: null,
    settingsMaxDate: null,
    settingsMaxTime: null,
    settingsTickType: null,
  };

  get map() {
    return window.OL;
  }

  constructor(props) {
    super(props);

    this.tickIntervals = [{
      value: 4000,
      name: '0.25x',
    },{
      value: 2000,
      name: '0.5x',
    },{
      value: 1000,
      name: '1x',
    },{
      value: 500,
      name: '2x',
    }];

    this.timerId = null;
    this.updateLayersDebounce = debounce(this.updateLayers, 250);

    this.minValueCalendarRef = React.createRef();
    this.maxValueCalendarRef = React.createRef();
  }

  componentDidMount() {
  };

  componentWillUnmount() {
  };

  componentDidUpdate(prevProps, prevState, snapshot) {
    if (prevProps.layers !== this.props.layers) {
      const {currentValue} = this.state;
      const range = this.getValueRange();
      if (!currentValue && range.min) {
        this.setState({
          currentValue: range.min,
        }, () => {
          this.updateLayers();
        });
      }
    }

    if (prevProps.playback !== this.props.playback) {
      this.setState({
        tickType: this.props.playback.frequency,
      });
    }

    if (prevState.settingsPanel !== this.state.settingsPanel) {
      if (this.state.settingsPanel) {
        setTimeout(() => {
          this.setState({
            settingsPanel2: true,
          });
        }, 1000);
      }
      else {
        this.setState({
          settingsPanel2: false,
        });
      }
    }
  }


  getValueRange = () => {
    const range = {
      min: null,
      max: null,
      minNumber: 0,
      maxNumber: 0,
    };

    const {playback} = this.props;

    /*
    if (layers) {
      const playbackLayers = layersHelper.toArray(layers).filter((item) => item.playback);
      range.max = moment.max(playbackLayers.map(item => item.playback.maxValue));
      range.min = moment.min(playbackLayers.map(item => item.playback.minValue));

      range.maxNumber = parseInt(range.max.format('x'));
      range.minNumber = parseInt(range.min.format('x'));
    }
    */

    if (playback) {
      if (playback.relative && !playback.min && !playback.max) {
        playback.max = moment();
        playback.min = moment().add(-playback.relative.value, playback.relative.type);
      }
      range.max =  playback.max;
      range.min = playback.min;
      range.maxNumber = parseInt(playback.max.format('x'));
      range.minNumber = parseInt(playback.min.format('x'));
    }

    const {minValue, maxValue} = this.state;
    if (minValue) {
      range.min = minValue;
      range.minNumber = parseInt(range.min.format('x'));
    }
    if (maxValue) {
      range.max = maxValue;
      range.maxNumber = parseInt(range.max.format('x'));
    }

    return range;
  };

  updateLayersTimer = (play) => {
    if (!play && this.timerId === null) {
      return;
    }

    const {tickIntervalIndex, tickValue, tickType, currentValue, direction} = this.state;
    const tickInterval = this.tickIntervals[tickIntervalIndex];

    let newCurrentValue = currentValue.clone().add(tickValue * direction, tickType);
    const range = this.getValueRange();

    if (range.min && range.min > newCurrentValue) {
      newCurrentValue = range.min;
    }
    else if (range.max && range.max < newCurrentValue) {
      newCurrentValue = range.max;
    }

    this.updateLayers(newCurrentValue);

    this.setState({
      currentValue: newCurrentValue,
    });

    this.timerId = setTimeout(this.updateLayersTimer, tickInterval.value);
  };
  updateLayers = (value) => {
    if (!value) {
      value = this.state.currentValue;
    }

    const {layers} = this.props;
    if (layers) {
      const playbackLayers = layersHelper.toArray(layers).filter((item) => item.playback);
      playbackLayers.map(layer => {
        layer.playback.currentValue = value;
        layer.olSource.setUrl(layer.layerUrl + '&viewparams=' + layer.playback.param + ':' + layer.playback.currentValue.format('YYYYMMDDTHHmmss'));
      });
    }
  };

  onPlay = () => {
    this.setState({
      play: true,
      settingsPanel: false,
    });

    this.updateLayersTimer(true);
  };
  onStop = () => {
    this.setState({
      play: false,
      settingsPanel: false,
    });

    if (this.timerId) {
      clearTimeout(this.timerId);
      this.timerId = null;
    }
  };

  onBackward = () => {
    this.setState({
      direction: -1,
      settingsPanel: false,
    });
  };
  onForward = () => {
    this.setState({
      direction: 1,
      settingsPanel: false,
    });
  };

  onToTheStart = () => {
    const ranges = this.getValueRange();
    this.setState({
      currentValue: ranges.min === null ? null : ranges.min.clone(),
      settingsPanel: false,
    }, () => {
      this.updateLayers();
    });
  };
  onToTheEnd = () => {
    const ranges = this.getValueRange();
    this.setState({
      currentValue: ranges.max === null ? null : ranges.max.clone(),
      settingsPanel: false,
    }, () => {
      this.updateLayers();
    });
  };

  onSliderChange = (value) => {
    const m = moment(value);
    this.setState({
      currentValue: m,
      settingsPanel: false,
    });

    this.updateLayersDebounce(m);
  };

  onSpeedMinus = () => {
    const {tickIntervalIndex} = this.state;
    if (tickIntervalIndex === 0) {
      return;
    }

    this.setState({
      tickIntervalIndex: tickIntervalIndex - 1,
    });
  };
  onSpeedPlus = () => {
    const {tickIntervalIndex} = this.state;
    if (tickIntervalIndex + 1 >= this.tickIntervals.length) {
      return;
    }

    this.setState({
      tickIntervalIndex: tickIntervalIndex + 1,
    });
  };


  onSettingsPanel = () => {
    const {settingsPanel, tickType} = this.state;
    const range = this.getValueRange();

    this.setState({
      settingsPanel: !settingsPanel,
      settingsMinDate: range.min ? range.min.format('DD.MM.YYYY') : '',
      settingsMinTime: range.min ? range.min.format('HH:mm:ss') : '',
      settingsMaxDate: range.max ? range.max.format('DD.MM.YYYY') : '',
      settingsMaxTime: range.max ? range.max.format('HH:mm:ss') : '',
      settingsTickType: tickType,
    });
  };

  onSettingsClose = () => {
    this.setState({
      settingsPanel: false,
    });
  };

  onSettingsOk = () => {
    const {settingsMinDate, settingsMinTime, settingsMaxDate, settingsMaxTime, settingsTickType} = this.state;

    const minValue = moment(settingsMinDate + ' ' + settingsMinTime, 'DD.MM.YYYY HH:mm:ss');
    const maxValue = moment(settingsMaxDate + ' ' + settingsMaxTime, 'DD.MM.YYYY HH:mm:ss');
    if (!minValue.isValid() || !maxValue.isValid()) {
      return;
    }

    this.setState({
      settingsPanel: false,
      minValue: minValue,
      maxValue: maxValue,
      tickType: settingsTickType,
    });
  };

  onSettingsReset = () => {
    const {playback} = this.props;
    if (playback.relative) {
      delete playback.min;
      delete playback.max;
    }

    this.setState({
      settingsPanel: false,
      minValue: null,
      maxValue: null,
    }, () => {
      const range = this.getValueRange();
      this.setState({
        currentValue: range.min,
        settingsMinDate: range.min ? range.min.format('DD.MM.YYYY') : '',
        settingsMinTime: range.min ? range.min.format('HH:mm:ss') : '',
        settingsMaxDate: range.max ? range.max.format('DD.MM.YYYY') : '',
        settingsMaxTime: range.max ? range.max.format('HH:mm:ss') : '',
        settingsTickType: playback.frequency,
        tickType: playback.frequency,
      }, () => {
        this.updateLayers();
      });
    });
  };

  onMinValueDateChange = (event) => {
    this.setState({
      settingsMinDate: event.target.value,
    });
  };
  onMinValueDatePickerChange = (value) => {
    this.setState({
      settingsMinDate: moment(value).format('DD.MM.YYYY'),
    });
  };
  onMinValueTimeChange = (event) => {
    this.setState({
      settingsMinTime: event.target.value,
    });
  };
  onMaxValueDateChange = (event) => {
    this.setState({
      settingsMaxDate: event.target.value,
    });
  };
  onMaxValueDatePickerChange = (value) => {
    this.setState({
      settingsMaxDate: moment(value).format('DD.MM.YYYY'),
    });
  };
  onMaxValueTimeChange = (event) => {
    this.setState({
      settingsMaxTime: event.target.value,
    });
  };

  onTickTypeChange = (option) => {
    this.setState({
      settingsTickType: option.value,
    });
  };


  render() {
    const {visible, playback} = this.props;
    const {settingsPanel, settingsPanel2, currentValue, play, direction, tickIntervalIndex, settingsMinDate, settingsMinTime, settingsMaxDate, settingsMaxTime, settingsTickType} = this.state;
    const tickInterval = this.tickIntervals[tickIntervalIndex];
    const className = visible ? 'gis-timelapse-panel' : 'gis-timelapse-panel off';
    const settingsPanelClassName = settingsPanel ? 'gis-timelapse-settings-panel visible' : 'gis-timelapse-settings-panel off';

    const range = this.getValueRange();

    const tickTypeOptions = [
    ];
    tickTypeOptions.push({ value: 'seconds', label: 'ежесекундно' });
    if (playback.frequency === 'minutes') {
      tickTypeOptions.push({ value: 'minutes', label: 'ежеминутно' });
    } else {
      tickTypeOptions.push({ value: 'minutes', label: 'ежеминутно' });
      if (playback.frequency === 'hours') {
        tickTypeOptions.push({ value: 'hours', label: 'ежечасно' });
      } else {
        tickTypeOptions.push({ value: 'hours', label: 'ежечасно' });
        if (playback.frequency === 'days') {
          tickTypeOptions.push({ value: 'days', label: 'ежедневно' });
        } else {
          tickTypeOptions.push({ value: 'days', label: 'ежедневно' });
          if (playback.frequency === 'weeks') {
            tickTypeOptions.push({ value: 'weeks', label: 'еженедельно' });
          } else {
            tickTypeOptions.push({ value: 'weeks', label: 'еженедельно' });
            if (playback.frequency === 'months') {
              tickTypeOptions.push({ value: 'months', label: 'ежемесячно' });
            } else {
              tickTypeOptions.push({ value: 'months', label: 'ежемесячно' });
              if (playback.frequency === 'years') {
                tickTypeOptions.push({ value: 'years', label: 'ежегодно' });
              }
            }
          }
        }
      }
    }

    return (
      <div>
        <div className={className}>
          <div className={'time'}>
            <div className={'min'}>
              {range.min ? range.min.format('HH:mm') : null}
              <br/>
              {range.min ? range.min.format('DD.MM.YYYY') : null}
            </div>

            <div className={'current'}>
              {currentValue  ? currentValue .format('HH:mm') : null}
              <br/>
              {currentValue  ? currentValue .format('DD.MM.YYYY') : null}
            </div>

            <div className={'max'}>
              {range.max ? range.max.format('HH:mm') : null}
              <br/>
              {range.max ? range.max.format('DD.MM.YYYY') : null}
            </div>
          </div>
          <Slider onChange={this.onSliderChange} value={currentValue ? parseInt(currentValue.format('x')) : null} min={range.minNumber} max={range.maxNumber} />
          <div className={'actions'}>
            <div className={'gis-speed-value'}>{tickInterval.name}</div>
            <a title={'-'} className={'gis-speed-minus' + (tickIntervalIndex === 0 ? ' disabled' : '')} onClick={this.onSpeedMinus}>
              <span className='icon'>
                <IcoSpeedMinus />
              </span>
            </a>
            <a title={'+'} className={'gis-speed-plus' + (tickIntervalIndex + 1 >= this.tickIntervals.length ? ' disabled' : '')} onClick={this.onSpeedPlus}>
              <span className='icon'>
                <IcoSpeedPlus />
              </span>
            </a>

            <a title={'В начало'} className={'gis-start'} onClick={this.onToTheStart}>
              <span className='icon'>
                <IcoFromTheStart />
              </span>
            </a>
            <a title={'Назад'} className={'gis-backward' + (play && direction === -1 ? ' active' : '')} onClick={this.onBackward}>
              <span className='icon'>
                <IcoBackward />
              </span>
            </a>
            {play ? <a title={'Остановить'} className={'gis-stop'} onClick={this.onStop}>
              <span className='icon'>
                <IcoStop />
              </span>
            </a> : <a title={'Запустить'} className={'gis-play'} onClick={this.onPlay}>
              <span className='icon'>
                <IcoPlay />
              </span>
            </a>}
            <a title={'Вперед'} className={'gis-forward' + (play && direction === 1 ? ' active' : '')} onClick={this.onForward}>
              <span className='icon'>
                <IcoForward />
              </span>
            </a>
            <a title={'В конец'} className={'gis-end'} onClick={this.onToTheEnd}>
              <span className='icon'>
                <IcoToTheEnd />
              </span>
            </a>
            <a title={'Настройки'} className={'gis-settings' + (settingsPanel ? ' active' : '')} onClick={this.onSettingsPanel}>
              <span className='icon'>
                <IcoSettings />
              </span>
            </a>
          </div>
        </div>
        <div className={settingsPanelClassName + ' ' + (settingsPanel2 ? 'visible2' : '')}>
          <div className={'header'}>
            Настройка воспроизведения
            <a className={'close'} onClick={this.onSettingsClose}></a>
          </div>
          <div className={'content'}>
            <div className={'param'}>
              <div className={'param-name'}>
                Дата и время начала
                <a className={'reset'} onClick={this.onSettingsReset}>Сбросить всё</a>
              </div>
              <DatePicker
                showPopperArrow={false}
                locale="ru"
                dateFormat="dd.MM.yyyy"
                selected={moment(settingsMinDate, 'DD.MM.YYYY').isValid() ? moment(settingsMinDate, 'DD.MM.YYYY').toDate(): null}
                onChange={this.onMinValueDatePickerChange}
                autoFocus
                customInput={<ReactCalendarInput
                  onChangeText={this.onMinValueDateChange}
                  value={settingsMinDate}
                />}
                shouldCloseOnSelect={true}
                ref={this.minValueCalendarRef}
              />
              <input className={'param-time'} type='text' value={settingsMinTime === null ? '' : settingsMinTime} placeholder='' onChange={this.onMinValueTimeChange}/>
            </div>
            <div className={'param'}>
              <div className={'param-name'}>Дата и время окончания</div>
              <div className={'param-date'}>
                <DatePicker
                  showPopperArrow={false}
                  locale="ru"
                  dateFormat="dd.MM.yyyy"
                  selected={moment(settingsMaxDate, 'DD.MM.YYYY').isValid() ? moment(settingsMaxDate, 'DD.MM.YYYY').toDate(): null}
                  onChange={this.onMaxValueDatePickerChange}
                  autoFocus
                  customInput={<ReactCalendarInput
                    onChangeText={this.onMaxValueDateChange}
                    value={settingsMaxDate}
                  />}
                  shouldCloseOnSelect={true}
                  ref={this.maxValueCalendarRef}
                />
              </div>
              <input className={'param-time'} type='text' value={settingsMaxTime === null ? '' : settingsMaxTime} placeholder='' onChange={this.onMaxValueTimeChange}/>
            </div>
            <div className={'param'}>
              <div className={'param-name'}>Частота изменения данных</div>
              <Select
                value={tickTypeOptions.find((x) => x.value === settingsTickType)}
                options={tickTypeOptions}
                className={'arrow-end-size'}
                classNamePrefix={'map-report'}
                onChange={this.onTickTypeChange}
              />
            </div>
          </div>
          <div className={'footer'}>
            <a className={'ok-btn'} onClick={this.onSettingsOk}>Применить</a>
            <span className={'delimiter'}></span>
            <a className={'cancel-btn'} onClick={this.onSettingsClose}>Отменить</a>
          </div>
        </div>
      </div>
    );
  }
}

TimelapsePanel.propTypes = {
  visible: PropTypes.bool,
  layers: PropTypes.oneOfType([
    PropTypes.array,
    PropTypes.bool,
  ]),
  playback: PropTypes.any,
};
