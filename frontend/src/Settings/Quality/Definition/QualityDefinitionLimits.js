import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';

function QualityDefinitionLimits(props) {
  const {
    bytes,
    message
  } = props;

  if (!bytes) {
    return <div>{message}</div>;
  }

  // Sizes are for the whole release, games have no runtime
  return (
    <div>
      {formatBytes(bytes)}
    </div>
  );
}

QualityDefinitionLimits.propTypes = {
  bytes: PropTypes.number,
  message: PropTypes.string.isRequired
};

export default QualityDefinitionLimits;
