import React, { useMemo } from 'react';
import Label, { LabelProps } from 'Components/Label';
import ClipboardButton from 'Components/Link/ClipboardButton';
import Link from 'Components/Link/Link';
import Game from 'Game/Game';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './GameDetailsLinks.css';

type GameDetailsLinksProps = Pick<
  Game,
  'igdbId' | 'imdbId' | 'youTubeTrailerId'
>;

interface GameDetailsLink {
  externalId?: string | number;
  name: string;
  url: string;
  kind?: LabelProps['kind'];
}

function GameDetailsLinks(props: GameDetailsLinksProps) {
  const { igdbId, imdbId, youTubeTrailerId } = props;

  const links = useMemo(() => {
    const validLinks: GameDetailsLink[] = [];

    if (igdbId) {
      validLinks.push(
        {
          externalId: igdbId,
          name: 'IGDB',
          url: `https://www.thegamedb.org/game/${igdbId}`,
        },
        {
          name: 'Letterboxd',
          url: `https://letterboxd.com/igdb/${igdbId}`,
        }
      );
    }

    if (imdbId) {
      validLinks.push(
        {
          externalId: imdbId,
          name: 'IMDb',
          url: `https://imdb.com/title/${imdbId}/`,
        },
        {
          name: 'Trakt',
          url: `https://trakt.tv/games/${imdbId}`,
        },
        {
          name: 'Game Chat',
          url: `https://gamechat.org/${imdbId}/`,
        },
        {
          name: 'MDBList',
          url: `https://mdblist.com/game/${imdbId}`,
        },
        {
          name: 'Blu-ray',
          url: `https://www.blu-ray.com/search/?quicksearch=1&quicksearch_keyword=${imdbId}&section=theatrical`,
        }
      );
    }

    if (youTubeTrailerId) {
      validLinks.push({
        name: translate('Trailer'),
        url: `https://www.youtube.com/watch?v=${youTubeTrailerId}`,
        kind: kinds.DANGER,
      });
    }

    return validLinks.sort(
      (a, b) => Number(!a.externalId) - Number(!b.externalId)
    );
  }, [igdbId, imdbId, youTubeTrailerId]);

  return (
    <div className={styles.links}>
      {links.map((link) => (
        <div key={link.name} className={styles.linkBlock}>
          <Link className={styles.link} to={link.url}>
            <Label
              className={styles.linkLabel}
              kind={link.kind ?? kinds.INFO}
              size={sizes.LARGE}
            >
              {link.name}
            </Label>
          </Link>

          {link.externalId ? (
            <ClipboardButton
              value={`${link.externalId}`}
              title={translate('CopyToClipboard')}
              kind={kinds.DEFAULT}
              size={sizes.SMALL}
              label={link.externalId}
            />
          ) : null}
        </div>
      ))}
    </div>
  );
}

export default GameDetailsLinks;
