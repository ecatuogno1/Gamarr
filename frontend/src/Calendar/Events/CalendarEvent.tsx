import classNames from 'classnames';
import moment from 'moment';
import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { GamePlatform } from 'Game/Game';
import { getTitleWithPlatform } from 'Game/Title/gameTitle';
import useGameFile from 'GameFile/useGameFile';
import { icons, kinds } from 'Helpers/Props';
import { createQueueItemSelectorForHook } from 'Store/Selectors/createQueueItemSelector';
import translate from 'Utilities/String/translate';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import getCalendarStatusLabel from './getCalendarStatusLabel';
import styles from './CalendarEvent.css';

interface CalendarEventProps {
  id: number;
  gameFileId?: number;
  title: string;
  titleSlug: string;
  genres: string[];
  platform?: GamePlatform;
  date: string;
  releaseDate?: string;
  isAvailable: boolean;
  monitored: boolean;
  hasFile: boolean;
  grabbed?: boolean;
}

function CalendarEvent({
  id,
  gameFileId,
  title,
  titleSlug,
  genres = [],
  platform,
  date,
  releaseDate,
  isAvailable,
  monitored: isMonitored,
  hasFile,
  grabbed,
}: CalendarEventProps) {
  const gameFile = useGameFile(gameFileId);
  const queueItem = useSelector(createQueueItemSelectorForHook(id));

  const {
    showGameInformation,
    showReleaseDate,
    showCutoffUnmetIcon,
    fullColorEvents,
  } = useSelector((state: AppState) => state.calendar.options);

  const isDownloading = !!(queueItem || grabbed);
  const statusStyle = getStatusStyle(
    hasFile,
    isDownloading,
    isMonitored,
    isAvailable
  );
  const joinedGenres = genres.slice(0, 2).join(', ');
  const link = `/game/${titleSlug}`;

  const eventTypes = useMemo(() => {
    const momentDate = moment(date);

    const types = [];

    if (
      showReleaseDate &&
      releaseDate &&
      momentDate.isSame(moment(releaseDate), 'day')
    ) {
      types.push('Release');
    }

    return types;
  }, [date, showReleaseDate, releaseDate]);

  return (
    <div
      className={classNames(
        styles.event,
        styles[statusStyle],
        fullColorEvents && 'fullColor'
      )}
    >
      <Link
        className={styles.underlay}
        aria-label={translate('CalendarOpenGame', {
          title,
          releaseTypes: eventTypes.join(', '),
          status: getCalendarStatusLabel(statusStyle),
        })}
        to={link}
      />

      <div className={styles.overlay}>
        <div className={styles.info}>
          <div className={styles.gameTitle}>
            {getTitleWithPlatform(title, platform)}
          </div>

          <div
            className={classNames(
              styles.statusContainer,
              fullColorEvents && 'fullColor'
            )}
          >
            {queueItem ? (
              <span className={styles.statusIcon}>
                <CalendarEventQueueDetails {...queueItem} />
              </span>
            ) : null}

            {!queueItem && grabbed ? (
              <Icon
                className={styles.statusIcon}
                name={icons.DOWNLOADING}
                title={translate('GameIsDownloading')}
              />
            ) : null}

            {showCutoffUnmetIcon &&
            !!gameFile &&
            gameFile.qualityCutoffNotMet ? (
              <Icon
                className={styles.statusIcon}
                name={icons.CUTOFF_NOT_MET}
                kind={kinds.WARNING}
                title={translate('QualityCutoffNotMet')}
              />
            ) : null}
          </div>
        </div>

        {showGameInformation ? (
          <>
            {joinedGenres ? (
              <div className={styles.gameInfo}>
                <div className={styles.genres}>{joinedGenres}</div>
              </div>
            ) : null}

            <div className={styles.gameInfo}>
              <div className={styles.eventType}>{eventTypes.join(', ')}</div>
            </div>
          </>
        ) : null}
      </div>
    </div>
  );
}

export default CalendarEvent;
