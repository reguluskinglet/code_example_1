import proj4 from 'proj4';

export default () => {
  proj4.defs(
    'EPSG:202474',
    '+proj=tmerc +lat_0=0 +lon_0=30 +k=1 +x_0=95942.17 +y_0=-6552810 +ellps=krass +towgs84=23.92,-141.27,-80.9,0,0.19,0.81,-0.12 +units=m +no_defs',
  );
};

