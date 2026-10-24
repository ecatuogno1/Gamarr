import React from 'react';
import CriticRating from 'Components/CriticRating';
import GameTagList from 'Components/GameTagList';
import Icon from 'Components/Icon';
import IgdbRating from 'Components/IgdbRating';
import { Ratings } from 'Game/Game';
import { icons } from 'Helpers/Props';
import Language from 'Language/Language';
import QualityProfile from 'typings/QualityProfile';
import formatDate from 'Utilities/Date/formatDate';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './GameIndexPosterInfo.css';

interface GameIndexPosterInfoProps {
  platform?: string;
  showQualityProfile: boolean;
  qualityProfile?: Pick<QualityProfile, 'name'>;
  added?: string;
  year: number;
  releaseDate?: string;
  path: string;
  ratings: Ratings;
  originalTitle: string;
  originalLanguage: Language;
  sizeOnDisk?: number;
  tags: number[];
  sortKey: string;
  showRelativeDates: boolean;
  showReleaseDate: boolean;
  shortDateFormat: string;
  longDateFormat: string;
  timeFormat: string;
  showIgdbRating: boolean;
  showCriticRating: boolean;
  showTags: boolean;
}

function GameIndexPosterInfo(props: GameIndexPosterInfoProps) {
  const {
    platform,
    showQualityProfile,
    qualityProfile,
    added,
    year,
    releaseDate,
    path,
    ratings,
    originalTitle,
    originalLanguage,
    sizeOnDisk = 0,
    tags = [],
    sortKey,
    showRelativeDates,
    showReleaseDate,
    shortDateFormat,
    longDateFormat,
    timeFormat,
    showIgdbRating,
    showCriticRating,
    showTags,
  } = props;

  if (sortKey === 'platform' && platform) {
    return (
      <div className={styles.info} title={translate('Platform')}>
        {platform}
      </div>
    );
  }

  if (
    sortKey === 'qualityProfileId' &&
    !showQualityProfile &&
    !!qualityProfile?.name
  ) {
    return (
      <div className={styles.info} title={translate('QualityProfile')}>
        {qualityProfile.name}
      </div>
    );
  }

  if (sortKey === 'added' && added) {
    const addedDate = getRelativeDate({
      date: added,
      shortDateFormat,
      showRelativeDates,
      timeFormat,
      timeForToday: false,
    });

    return (
      <div
        className={styles.info}
        title={formatDateTime(added, longDateFormat, timeFormat)}
      >
        {translate('Added')}: {addedDate}
      </div>
    );
  }

  if (sortKey === 'year' && year) {
    return (
      <div className={styles.info} title={translate('Year')}>
        {year}
      </div>
    );
  }

  if (sortKey === 'releaseDate' && releaseDate && !showReleaseDate) {
    return (
      <div
        className={styles.info}
        title={`${translate('ReleaseDate')}: ${formatDate(
          releaseDate,
          longDateFormat
        )}`}
      >
        <Icon name={icons.CALENDAR} />{' '}
        {getRelativeDate({
          date: releaseDate,
          shortDateFormat,
          showRelativeDates,
          timeFormat,
          timeForToday: false,
        })}
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

  if (!showTags && sortKey === 'tags' && tags.length) {
    return (
      <div className={styles.tags}>
        <div className={styles.tagsList}>
          <GameTagList tags={tags} />
        </div>
      </div>
    );
  }

  if (sortKey === 'path') {
    return (
      <div className={styles.info} title={translate('Path')}>
        {path}
      </div>
    );
  }

  if (sortKey === 'sizeOnDisk') {
    return (
      <div className={styles.info} title={translate('SizeOnDisk')}>
        {formatBytes(sizeOnDisk)}
      </div>
    );
  }

  if (sortKey === 'originalTitle' && originalTitle) {
    return (
      <div className={styles.title} title={originalTitle}>
        {originalTitle}
      </div>
    );
  }

  if (sortKey === 'originalLanguage' && originalLanguage) {
    return (
      <div className={styles.info} title={translate('OriginalLanguage')}>
        {originalLanguage.name}
      </div>
    );
  }

  return null;
}

export default GameIndexPosterInfo;
