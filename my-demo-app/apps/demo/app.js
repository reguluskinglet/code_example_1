import 'babel-polyfill';

import * as React from 'react';
import {NavLink, Switch, Route} from "react-router-dom";
import {Layout, Menu} from 'antd';

import {
  TokenMode,
  MapMode,
  EnterMode,
  FixedMode,
  SearchMode,
  SelectMode,
  SelectModeCustom,
  ReportMode,
  TimelapseMode,
  MeasuresMode,
  PrintExportMode,
  StyleExamples,
  ExtraLayersMode,
  ErrorMode,
} from './components';

const {SubMenu} = Menu;
const {Content, Sider} = Layout;

import './style.scss';

export class App extends React.Component {
  constructor(props) {
    super(props)
  }

  render() {
    return (
      <Layout style={{height: "100vh"}}>
        <Sider width={300} style={{background: '#fff'}}>
          <Menu
            mode="inline"
            defaultOpenKeys={['sub1']}
            style={{height: '100%', borderRight: 0}}
          >
            <Menu.Item key="token-mode">
              <NavLink to="/token-mode" className="nav-text">
                Авторизация
              </NavLink>
            </Menu.Item>
            <SubMenu key="sub1" title="Картографический компонент">
              <Menu.Item key="map-mode">
                <NavLink to="/map-mode" className="nav-text">
                  Карта
                </NavLink>
              </Menu.Item>
              <Menu.Item key="fixed-mode">
                <NavLink to="/fixed-mode" className="nav-text">
                  Карта с фиксированным размером
                </NavLink>
              </Menu.Item>
              <Menu.Item key="style-examples">
                <NavLink to="/style-examples" className="nav-text">
                  Стили
                </NavLink>
              </Menu.Item>
              <Menu.Item key="search-mode">
                <NavLink to="/search-mode" className="nav-text">
                  Поиск на карте
                </NavLink>
              </Menu.Item>
              <Menu.Item key="measures-mode">
                <NavLink to="/measures-mode" className="nav-text">
                  Режим измерений
                </NavLink>
              </Menu.Item>
              <Menu.Item key="print-export-mode">
                <NavLink to="/print-export-mode" className="nav-text">
                  Режим экспорта/печати
                </NavLink>
              </Menu.Item>
              <Menu.Item key="enter-mode">
                <NavLink to="/enter-mode" className="nav-text">
                  Режим ввода
                </NavLink>
              </Menu.Item>
              <Menu.Item key="select-mode">
                <NavLink to="/select-mode" className="nav-text">
                  Режим выбора
                </NavLink>
              </Menu.Item>
              <Menu.Item key="select-mode-custom">
                <NavLink to="/select-mode-custom" className="nav-text">
                  Режим выбора (Custom)
                </NavLink>
              </Menu.Item>
              <Menu.Item key="report-mode">
                <NavLink to="/report-mode" className="nav-text">
                  Режим оформления отчета
                </NavLink>
              </Menu.Item>
              <Menu.Item key="timelapse-mode">
                <NavLink to="/timelapse-mode" className="nav-text">
                  Режим воспроизведения
                </NavLink>
              </Menu.Item>
              <Menu.Item key="extra-layers-mode">
                <NavLink to="/extra-layers-mode" className="nav-text">
                  Дополнительные слои
                </NavLink>
              </Menu.Item>
              <Menu.Item key="error-mode">
                <NavLink to="/error-mode" className="nav-text">
                  Обработка ошибок
                </NavLink>
              </Menu.Item>
            </SubMenu>
            <SubMenu key="sub2" title="Табличный компонент">
            </SubMenu>
          </Menu>
        </Sider>
        <Layout style={{background: '#fff'}}>
          <Content style={{background: '#fff', padding: '0 24px 0 24px', margin: 0, minHeight: 280, height: '100%'}}>
            <Switch>
              <Route exact path='/token-mode' component={TokenMode}/>
              <Route exact path='/map-mode' component={MapMode}/>
              <Route exact path='/fixed-mode' component={FixedMode}/>
              <Route exact path='/style-examples' component={StyleExamples}/>
              <Route exact path='/search-mode' component={SearchMode}/>
              <Route exact path='/enter-mode' component={EnterMode}/>
              <Route exact path='/select-mode' component={SelectMode}/>
              <Route exact path='/select-mode-custom' component={SelectModeCustom}/>
              <Route exact path='/report-mode' component={ReportMode}/>
              <Route exact path='/timelapse-mode' component={TimelapseMode}/>
              <Route exact path='/measures-mode' component={MeasuresMode}/>
              <Route exact path='/print-export-mode' component={PrintExportMode}/>
              <Route exact path='/extra-layers-mode' component={ExtraLayersMode}/>
              <Route exact path='/error-mode' component={ErrorMode}/>
            </Switch>
          </Content>
        </Layout>
      </Layout>
    )
  }
}
