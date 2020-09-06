import React, { forwardRef } from 'react';

const ReactCalendarInput = (props, ref) => {
  const {
    onClick,
    onChangeText,
    value,
  } = props;

  return (
    <div className={'param-date'}>
      <input type='text' value={value} placeholder='' onChange={onChangeText}/>
      <div className={'param-date-btn'} onClick={() => {onClick();}}></div>
    </div>
  );
};

export default forwardRef(ReactCalendarInput);