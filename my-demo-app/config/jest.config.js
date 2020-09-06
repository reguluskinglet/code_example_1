module.exports = {
  collectCoverageFrom: [
    'src/**/*.{js,jsx}',
    '!src/**/*.test.{js,jsx}',
    '!src/*/RbGenerated*/*.{js,jsx}',
    '!src/app.js',
    '!src/*/*/Loadable.{js,jsx}'
  ],
  coverageThreshold: {
    global: {
      statements: 98,
      branches: 91,
      functions: 98,
      lines: 98
    }
  },
  coverageReporters: ['json', 'lcov', 'text-summary'],
  moduleDirectories: ['node_modules', 'src'],
  moduleNameMapper: {
    '.*\\.(css|less|styl|scss|sass)$':
      'identity-obj-proxy',
    '.*\\.(jpg|jpeg|png|gif|eot|otf|webp|svg|ttf|woff|woff2|mp4|webm|wav|mp3|m4a|aac|oga)$':
      '<rootDir>/config/jest-mocks/image.js'
  },
  setupTestFrameworkScriptFile: '<rootDir>/config/test-setup.js',
  testPathIgnorePatterns: [
    "<rootDir>/(build|docs|node_modules)/"
  ],
  transformIgnorePatterns: [
    "/node_modules/(?!(ol|antd|rc-util|rc-tabs)/).*/"
  ],
  testRegex: 'tests/.*\\.test\\.js$'
};