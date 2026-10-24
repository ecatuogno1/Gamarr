import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import Game from 'Game/Game';
import { kinds, sizes } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatDate from 'Utilities/Date/formatDate';
import translate from 'Utilities/String/translate';
import styles from './GameDetailsInfo.css';

const storeTypes = ['Steam', 'GOG', 'Epic', 'Itch'];

interface GameDetailsInfoProps {
  game: Game;
}

// Game specific details from IGDB: release dates per platform (the platform versions in the library are in the
// title page's platform switcher), series, themes, game modes, age ratings and store links.
function GameDetailsInfo({ game }: GameDetailsInfoProps) {
  const {
    platform,
    platformReleaseDates = [],
    themes = [],
    gameModes = [],
    ageRatings = [],
    websites = [],
  } = game;

  const platforms = useSelector(
    (state: AppState) => state.settings.platforms.items
  );
  const { longDateFormat } = useSelector(createUISettingsSelector());

  const releaseDates = useMemo(() => {
    return platformReleaseDates
      .filter((d) => !!d.date)
      .map((d) => {
        const p = platforms.find(
          (item) => item.igdbPlatformId === d.igdbPlatformId
        );

        return {
          igdbPlatformId: d.igdbPlatformId,
          name: p?.name ?? `IGDB ${d.igdbPlatformId}`,
          date: d.date as string,
          isGamePlatform: d.igdbPlatformId === platform?.igdbPlatformId,
        };
      })
      .sort((a, b) => a.date.localeCompare(b.date));
  }, [platformReleaseDates, platforms, platform]);

  const stores = websites.filter((w) => storeTypes.includes(w.type));

  return (
    <DescriptionList>
      {releaseDates.length ? (
        <DescriptionListItem
          title={translate('ReleaseDates')}
          data={
            <div>
              {releaseDates.map((d) => (
                <div
                  key={d.igdbPlatformId}
                  className={d.isGamePlatform ? styles.gamePlatform : undefined}
                >
                  {d.name}: {formatDate(d.date, longDateFormat)}
                </div>
              ))}
            </div>
          }
        />
      ) : null}

      {themes.length ? (
        <DescriptionListItem
          title={translate('Themes')}
          data={themes.join(', ')}
        />
      ) : null}

      {gameModes.length ? (
        <DescriptionListItem
          title={translate('GameModes')}
          data={gameModes.join(', ')}
        />
      ) : null}

      {ageRatings.length ? (
        <DescriptionListItem
          title={translate('AgeRatings')}
          data={
            <div className={styles.labels}>
              {ageRatings.map((r) => (
                <Label
                  key={`${r.organization}-${r.rating}`}
                  kind={kinds.DEFAULT}
                  size={sizes.MEDIUM}
                >
                  {r.organization} {r.rating}
                </Label>
              ))}
            </div>
          }
        />
      ) : null}

      {stores.length ? (
        <DescriptionListItem
          title={translate('Stores')}
          data={
            <div className={styles.labels}>
              {stores.map((w) => (
                <Link key={w.url} to={w.url}>
                  <Label kind={kinds.PRIMARY} size={sizes.MEDIUM}>
                    {w.type}
                  </Label>
                </Link>
              ))}
            </div>
          }
        />
      ) : null}
    </DescriptionList>
  );
}

export default GameDetailsInfo;
