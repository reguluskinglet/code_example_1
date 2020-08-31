import hoistNonReactStatics from 'hoist-non-react-statics';
import React, {
  createContext,
  forwardRef
} from 'react';

const {Consumer, Provider} = createContext({});

export const OlConsumer = Consumer;
export const OlProvider = Provider;

export const withOl = (WrappedComponent) => {
  const WithOlComponent = (props, ref,) => (
    <Consumer>
      {(olContext) => (
        <WrappedComponent {...props} ol={olContext} ref={ref}/>
      )}
    </Consumer>
  );

  const name = WrappedComponent.displayName || WrappedComponent.name;
  WithOlComponent.displayName = `Ol(${name})`;

  const OlComponent = forwardRef(WithOlComponent);
  hoistNonReactStatics(OlComponent, WrappedComponent);

  return OlComponent;
};
