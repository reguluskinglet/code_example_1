import * as React from 'react';
import PropTypes from 'prop-types';

import {toPng, toJpeg} from 'html-to-image';
import jsPDF from 'jspdf'
import debounce from "lodash/debounce";

import './style.scss';

export default class ExportPanel extends React.PureComponent {
  state = {
  };

  constructor(props) {
    super(props);

    this.pngExport = debounce(this.pngExport, 500);
    this.jpgExport = debounce(this.jpgExport, 500);
    this.pdfExport = debounce(this.pdfExport, 500);
  }

  get map() {
    return window.OL;
  }

  buildExportOptions = () => ({
    filter: function(element) {
      return element.className ? element.className.indexOf('ol-control') === -1 : true;
    },
    backgroundColor: 'white',
  });

  pngExport = () => {
    const map = this.map;

    map.renderSync();

    toPng(map.getTargetElement(), this.buildExportOptions())
      .then(function(dataURL) {
        const link = document.getElementById('image-download');
        link.download = 'map.png';
        link.href = dataURL;
        link.click();
      });
  };
  jpgExport = () => {
    const map = this.map;

    map.renderSync();

    toJpeg(map.getTargetElement(), this.buildExportOptions())
      .then(function(dataURL) {
        const link = document.getElementById('image-download');
        link.download = 'map.jpg';
        link.href = dataURL;
        link.click();
      });
  };
  pdfExport = () => {
    const map = this.map;

    const format = 'a4';
    const resolution = 150;
    const dim = [297, 210];
    const width = Math.round(dim[0] * resolution / 25.4);
    const height = Math.round(dim[1] * resolution / 25.4);

    const exportOptions = this.buildExportOptions();
    exportOptions.width = width;
    exportOptions.height = height;
    toJpeg(map.getViewport(), exportOptions).then(function(dataUrl) {
      const pdf = new jsPDF('landscape', undefined, format);
      pdf.addImage(dataUrl, 'JPEG', 10, 10, dim[0], dim[1]);
      pdf.save('map.pdf');
    });
  };

  render() {
    const {visible} = this.props;

    const className = visible ? 'gis-export-panel' : 'gis-export-panel off';

    return (
      <div className={className} ref={(el) => this.element = el}>
        <a title='PNG' className={'gis-map-button png-export'} onClick={this.pngExport}>
          PNG
        </a>
        <a title='JPG' className={'gis-map-button jpg-export'} onClick={this.jpgExport}>
          JPG
        </a>
        <a title='PDF' className={'gis-map-button pdf-export'} onClick={this.pdfExport}>
          PDF
        </a>
      </div>
    );
  }
}

ExportPanel.propTypes = {
  visible: PropTypes.bool,
};