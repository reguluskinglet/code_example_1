/* eslint-disable global-require */

/**
 * Front-end middleware
 */
module.exports = (app, options) => {
  const isProd = process.env.NODE_ENV === 'production';

  if (process.env.NODE_ENV === 'production') {
    const addProdMiddlewares = require('./addProdMiddlewares');
    addProdMiddlewares(app, options);
  } else if (process.env.NODE_ENV === 'development') {
    const webpackConfig = require('../../config/webpack.dev.babel');
    const addDevMiddlewares = require('./addDevMiddlewares');
    addDevMiddlewares(app, webpackConfig);
  } else if (process.env.NODE_ENV === 'demo_dev') {
    const demoWebpackConfig = require('../../config/webpack.demo.babel');
    const addDevMiddlewares = require('./addDevMiddlewares');
    addDevMiddlewares(app, demoWebpackConfig);
  }

  return app;
};
