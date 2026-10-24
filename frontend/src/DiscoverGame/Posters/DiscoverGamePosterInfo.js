import PropTypes from 'prop-types';
import React from 'react';
import CriticRating from 'Components/CriticRating';
import Icon from 'Components/Icon';
import IgdbRating from 'Components/IgdbRating';
import getGameStatusDetails from 'Game/getGameStatusDetails';
import { icons } from 'Helpers/Props';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import styles from './DiscoverGamePosterInfo.css';

function DiscoverGamePosterInfo(props) {
  const {
    status,
    developers,
    releaseDate,
    ratings,
    sortKey,
    showRelativeDates,
    shortDateFormat,
    timeFormat,
    showIgdbRating,
    showCriticRating
  } = props;

  if (sortKey === 'status' && status) {
    return (
      <div className={styles.info} title={translate('Status')}>
        {getGameStatusDetails(status).title}
      </div>
    );
  }

  if (sortKey === 'releaseDate' && releaseDate) {
    const releaseDateText = getRelativeDate({
      date: releaseDate,
      shortDateFormat,
      showRelativeDates,
      timeFormat,
      timeForToday: false
    });

    return (
      <div className={styles.info} title={translate('ReleaseDate')}>
        <Icon name={icons.CALENDAR} /> {releaseDateText}
      </div>
    );
  }

  if (sortKey === 'developers' && developers && developers.length) {
    return (
      <div className={styles.info} title={translate('Developers')}>
        {developers.join(', ')}
      </div>
    );
  }

  if (!showIgdbRating && sortKey === 'igdbRating' && !!ratings.igdb) {
    return (
      <div className={styles.info}>
        <IgdbRating ratings={ratings} iconSize={12} />
      </div>
    );
  }

  if (!showCriticRating && sortKey === 'criticRating' && !!ratings.critic) {
    return (
      <div className={styles.info}>
        <CriticRating ratings={ratings} iconSize={12} />
      </div>
    );
  }

  return null;
}

DiscoverGamePosterInfo.propTypes = {
  status: PropTypes.string,
  developers: PropTypes.arrayOf(PropTypes.string),
  releaseDate: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  sortKey: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  showIgdbRating: PropTypes.bool.isRequired,
  showCriticRating: PropTypes.bool.isRequired
};

export default DiscoverGamePosterInfo;
