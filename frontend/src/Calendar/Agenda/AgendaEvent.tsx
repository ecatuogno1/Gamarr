import classNames from 'classnames';
import moment from 'moment';
import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import CalendarEventQueueDetails from 'Calendar/Events/CalendarEventQueueDetails';
import getCalendarStatusLabel from 'Calendar/Events/getCalendarStatusLabel';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { GamePlatform, Image } from 'Game/Game';
import GamePoster from 'Game/GamePoster';
import { getTitleWithPlatform } from 'Game/Title/gameTitle';
import useGameFile from 'GameFile/useGameFile';
import { icons, kinds } from 'Helpers/Props';
import { createQueueItemSelectorForHook } from 'Store/Selectors/createQueueItemSelector';
import translate from 'Utilities/String/translate';
import styles from './AgendaEvent.css';

interface AgendaEventProps {
  id: number;
  gameFileId: number;
  title: string;
  titleSlug: string;
  platform?: GamePlatform;
  images: Image[];
  genres: string[];
  releaseDate?: string;
  sortDate: moment.Moment;
  isAvailable: boolean;
  monitored: boolean;
  hasFile: boolean;
  grabbed?: boolean;
}

function AgendaEvent({
  id,
  gameFileId,
  title,
  titleSlug,
  platform,
  images,
  genres = [],
  releaseDate,
  sortDate,
  isAvailable,
  monitored: isMonitored,
  hasFile,
  grabbed,
}: AgendaEventProps) {
  const gameFile = useGameFile(gameFileId);
  const queueItem = useSelector(createQueueItemSelectorForHook(id));

  const { showCoverArt, showGameInformation, showCutoffUnmetIcon } =
    useSelector((state: AppState) => state.calendar.options);

  const { eventTitle, releaseIcon } = useMemo(() => {
    if (releaseDate && sortDate.isSame(moment(releaseDate), 'day')) {
      return {
        eventTitle: translate('ReleaseDate'),
        releaseIcon: icons.DISC,
      };
    }

    return {
      eventTitle: null,
      releaseIcon: null,
    };
  }, [releaseDate, sortDate]);

  const downloading = !!(queueItem || grabbed);
  const statusStyle = getStatusStyle(
    hasFile,
    downloading,
    isMonitored,
    isAvailable
  );
  const joinedGenres = genres.slice(0, 2).join(', ');
  const link = `/game/${titleSlug}`;
  const eventLabel = translate('CalendarOpenGame', {
    title,
    releaseTypes: eventTitle ?? '',
    status: getCalendarStatusLabel(statusStyle),
  });

  return (
    <div className={classNames(styles.event, styles[statusStyle])}>
      <Link className={styles.underlay} aria-label={eventLabel} to={link} />

      {showCoverArt ? (
        <GamePoster
          className={styles.cover}
          images={images}
          size={250}
          lazy={false}
        />
      ) : null}

      <div className={styles.overlay}>
        <div className={classNames(styles.eventWrapper, styles[statusStyle])}>
          <div className={styles.releaseIcon}>
            {releaseIcon ? (
              <Icon name={releaseIcon} title={eventTitle ?? undefined} />
            ) : null}
          </div>

          <div className={styles.gameTitle}>
            {getTitleWithPlatform(title, platform)}
          </div>

          <div className={styles.genres}>
            {showGameInformation ? joinedGenres : null}
          </div>

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

          {showCutoffUnmetIcon && gameFile && gameFile.qualityCutoffNotMet ? (
            <Icon
              className={styles.statusIcon}
              name={icons.CUTOFF_NOT_MET}
              kind={kinds.WARNING}
              title={translate('QualityCutoffNotMet')}
            />
          ) : null}
        </div>
      </div>
    </div>
  );
}

export default AgendaEvent;
