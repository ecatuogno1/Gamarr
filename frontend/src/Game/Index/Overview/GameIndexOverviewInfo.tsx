import classNames from 'classnames';
import React, { forwardRef, ReactNode } from 'react';
import { useSelector } from 'react-redux';
import createSortedTagsSelector from 'Store/Selectors/createSortedTagsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import QualityProfile from 'typings/QualityProfile';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './GameIndexOverviewInfo.css';

interface GameIndexOverviewInfoProps {
  showPlatform: boolean;
  showMonitored: boolean;
  showQualityProfile: boolean;
  showAdded: boolean;
  showPath: boolean;
  showSizeOnDisk: boolean;
  showTags: boolean;
  monitored: boolean;
  platform?: string;
  qualityProfile?: Pick<QualityProfile, 'name'>;
  added?: string;
  path: string;
  sizeOnDisk?: number;
  tags: number[];
  sortKey: string;
}

const selectSortedTags = createSortedTagsSelector();

const GameIndexOverviewInfo = forwardRef<
  HTMLDivElement,
  GameIndexOverviewInfoProps
>((props, ref) => {
  const {
    showPlatform,
    showMonitored,
    showQualityProfile,
    showAdded,
    showPath,
    showSizeOnDisk,
    showTags,
    monitored,
    platform,
    qualityProfile,
    added,
    path,
    sizeOnDisk = 0,
    tags,
    sortKey,
  } = props;

  const { shortDateFormat, showRelativeDates, longDateFormat, timeFormat } =
    useSelector(createUISettingsSelector());
  const tagList = useSelector(selectSortedTags);

  const chips: ReactNode[] = [];

  if (platform && (showPlatform || sortKey === 'platform')) {
    chips.push(
      <span key="platform" className={styles.chip}>
        {platform}
      </span>
    );
  }

  if (showMonitored || sortKey === 'monitored') {
    chips.push(
      <span key="monitored" className={styles.chip}>
        {monitored ? translate('Monitored') : translate('Unmonitored')}
      </span>
    );
  }

  if (
    qualityProfile?.name &&
    (showQualityProfile || sortKey === 'qualityProfileId')
  ) {
    chips.push(
      <span key="qualityProfile" className={styles.chip}>
        {qualityProfile.name}
      </span>
    );
  }

  if (added && (showAdded || sortKey === 'added')) {
    chips.push(
      <span
        key="added"
        className={styles.chip}
        title={formatDateTime(added, longDateFormat, timeFormat)}
      >
        <span className={styles.dateLabel}>{translate('Added')}</span>
        {getRelativeDate({
          date: added,
          shortDateFormat,
          showRelativeDates,
          timeFormat,
          timeForToday: true,
        })}
      </span>
    );
  }

  if (sizeOnDisk > 0 && (showSizeOnDisk || sortKey === 'sizeOnDisk')) {
    chips.push(
      <span key="sizeOnDisk" className={styles.chip}>
        {formatBytes(sizeOnDisk)}
      </span>
    );
  }

  if (showTags && tags.length > 0) {
    tagList
      .filter((tag) => tags.includes(tag.id))
      .forEach((tag) => {
        chips.push(
          <span
            key={`tag-${tag.id}`}
            className={classNames(styles.chip, styles.chipTag)}
          >
            {tag.label}
          </span>
        );
      });
  }

  if (path && (showPath || sortKey === 'path')) {
    chips.push(
      <span key="path" className={styles.chipPath} title={path}>
        {path}
      </span>
    );
  }

  if (chips.length === 0) {
    return null;
  }

  return (
    <div ref={ref} className={styles.chipStrip}>
      {chips}
    </div>
  );
});

export default GameIndexOverviewInfo;
