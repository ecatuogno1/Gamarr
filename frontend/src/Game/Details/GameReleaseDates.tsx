import React from 'react';
import { useSelector } from 'react-redux';
import Icon from 'Components/Icon';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import Game from 'Game/Game';
import { icons } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatDate from 'Utilities/Date/formatDate';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import styles from './GameReleaseDates.css';

type GameReleaseDatesProps = Pick<Game, 'igdbId' | 'igdbSlug' | 'releaseDate'>;

// Release date on the game's platform (IGDB first release date when the platform has none)
function GameReleaseDates({
  igdbId,
  igdbSlug,
  releaseDate,
}: GameReleaseDatesProps) {
  const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
    useSelector(createUISettingsSelector());

  if (!releaseDate) {
    return (
      <div>
        <div className={styles.dateIcon}>
          <Icon name={icons.MISSING} />
        </div>

        <InlineMarkdown
          data={translate('NoGameReleaseDatesAvailable', {
            url: igdbSlug
              ? `https://www.igdb.com/games/${igdbSlug}`
              : `https://www.igdb.com/search?q=${igdbId}`,
          })}
        />
      </div>
    );
  }

  return (
    <div
      title={`${translate('ReleaseDate')}: ${formatDate(
        releaseDate,
        longDateFormat
      )}`}
    >
      <div className={styles.dateIcon}>
        <Icon name={icons.CALENDAR} />
      </div>

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

export default GameReleaseDates;
