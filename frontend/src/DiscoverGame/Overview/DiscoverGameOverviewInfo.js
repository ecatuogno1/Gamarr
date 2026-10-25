import classNames from 'classnames';
import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import { useSelector } from 'react-redux';
import CriticRating from 'Components/CriticRating';
import Icon from 'Components/Icon';
import IgdbRating from 'Components/IgdbRating';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './DiscoverGameOverviewInfo.css';

// v5 chip strip (Game/Index/Overview/GameIndexOverviewInfo): list sources,
// library status, then the optional info fields as muted chips.

const rows = [
  {
    name: 'year',
    showProp: 'showYear',
    valueProp: 'year'
  },
  {
    name: 'developers',
    showProp: 'showPlatform',
    valueProp: 'developers'
  },
  {
    name: 'genres',
    showProp: 'showGenres',
    valueProp: 'genres'
  },
  {
    name: 'igdbRating',
    showProp: 'showIgdbRating',
    valueProp: 'ratings.igdb.value'
  },
  {
    name: 'criticRating',
    showProp: 'showCriticRating',
    valueProp: 'ratings.critic.value'
  }
];

function isVisible(row, props) {
  const {
    name,
    showProp,
    valueProp
  } = row;

  return _.has(props, valueProp) &&
    _.get(props, valueProp) !== null &&
    (props[showProp] || props.sortKey === name);
}

function renderChip(row, props) {
  const { name } = row;

  switch (name) {
    case 'year':
      return (
        <span key={name} className={styles.chip}
          title={translate('Year')}
        >
          {props.year}
        </span>
      );
    case 'developers':
      return props.developers && props.developers.length ?
        <span key={name} className={styles.chip}
          title={translate('Developers')}
        >
          {props.developers.join(', ')}
        </span> :
        null;
    case 'genres':
      return props.genres.length ?
        <span key={name} className={styles.chip}
          title={translate('Genres')}
        >
          {props.genres.slice(0, 2).join(', ')}
        </span> :
        null;
    case 'igdbRating':
      return (
        <span key={name} className={styles.chip}>
          <IgdbRating ratings={props.ratings} iconSize={11} />
        </span>
      );
    case 'criticRating':
      return (
        <span key={name} className={styles.chip}>
          <CriticRating ratings={props.ratings} iconSize={11} />
        </span>
      );
    default:
      return null;
  }
}

function DiscoverGameOverviewInfo(props) {
  const {
    lists,
    isExisting,
    isExcluded,
    isRecommendation,
    isPopular,
    isTrending
  } = props;

  const allImportLists = useSelector((state) => state.settings.importLists.items);

  const chips = [];

  if (isExisting) {
    chips.push(
      <span
        key="existing"
        className={classNames(styles.chip, styles.chipStatus, styles.existing)}
        title={translate('AlreadyInYourLibrary')}
      >
        {translate('Existing')}
      </span>
    );
  }

  if (isExcluded) {
    chips.push(
      <span
        key="excluded"
        className={classNames(styles.chip, styles.chipStatus, styles.excluded)}
        title={translate('GameAlreadyExcluded')}
      >
        {translate('Excluded')}
      </span>
    );
  }

  if (isRecommendation) {
    chips.push(
      <span
        key="recommended"
        className={classNames(styles.chip, styles.chipSource)}
        title={translate('GameIsRecommend')}
      >
        <Icon name={icons.RECOMMENDED} size={11} />
      </span>
    );
  }

  if (isPopular) {
    chips.push(
      <span key="popular" className={classNames(styles.chip, styles.chipSource)}>
        {translate('Popular')}
      </span>
    );
  }

  if (isTrending) {
    chips.push(
      <span key="trending" className={classNames(styles.chip, styles.chipSource)}>
        {translate('Trending')}
      </span>
    );
  }

  lists.forEach((id) => {
    const importList = allImportLists.find((list) => list.id === id);

    if (importList) {
      chips.push(
        <span
          key={`list-${id}`}
          className={classNames(styles.chip, styles.chipSource)}
        >
          {importList.name}
        </span>
      );
    }
  });

  rows.forEach((row) => {
    if (isVisible(row, props)) {
      const chip = renderChip(row, props);

      if (chip) {
        chips.push(chip);
      }
    }
  });

  if (!chips.length) {
    return null;
  }

  return (
    <div className={styles.chipStrip}>
      {chips}
    </div>
  );
}

DiscoverGameOverviewInfo.propTypes = {
  showYear: PropTypes.bool.isRequired,
  showPlatform: PropTypes.bool.isRequired,
  showGenres: PropTypes.bool.isRequired,
  showIgdbRating: PropTypes.bool.isRequired,
  showCriticRating: PropTypes.bool.isRequired,
  developers: PropTypes.arrayOf(PropTypes.string),
  year: PropTypes.number,
  ratings: PropTypes.object,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  lists: PropTypes.arrayOf(PropTypes.number).isRequired,
  isExisting: PropTypes.bool.isRequired,
  isExcluded: PropTypes.bool.isRequired,
  isRecommendation: PropTypes.bool.isRequired,
  isPopular: PropTypes.bool.isRequired,
  isTrending: PropTypes.bool.isRequired,
  sortKey: PropTypes.string.isRequired
};

DiscoverGameOverviewInfo.defaultProps = {
  genres: [],
  lists: []
};

export default DiscoverGameOverviewInfo;
