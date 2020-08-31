import * as React from 'react';
import {Tabs} from 'antd';

const TabPane = Tabs.TabPane;

import './style.scss';

export class TokenMode extends React.Component<any, any> {
  state = {
    token: localStorage['AuthToken'],
  };


  constructor(props) {
    super(props)
  }

  render() {
    return (
      <Tabs defaultActiveKey="1">
        <TabPane tab="Токен" key="1">
          <textarea style={{width: '100%'}} rows={1} value={this.state.token} onChange={(event) => { this.setState({token: event.target.value}); localStorage['AuthToken'] = event.target.value }} />

          <div style={{display: 'none'}}>
            <ul>
              <li>KGysjvwZg5m02N9nreKnGcS046wzEd-KHQAuBnuIToI</li>
            </ul>
          </div>
        </TabPane>
      </Tabs>
    );
  }
}