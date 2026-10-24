import React, { useMemo } from 'react';
import Label, { LabelProps } from 'Components/Label';
import ClipboardButton from 'Components/Link/ClipboardButton';
import Link from 'Components/Link/Link';
import Game from 'Game/Game';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './GameDetailsLinks.css';

type GameDetailsLinksProps = Pick<Game, 'igdbId' | 'youTubeTrailerId'> &
  Partial<Pick<Game, 'igdbSlug' | 'websites'>>;

interface GameDetailsLink {
  externalId?: string | number;
  name: string;
  url: string;
  kind?: LabelProps['kind'];
}

function GameDetailsLinks(props: GameDetailsLinksProps) {
  const { igdbId, igdbSlug, youTubeTrailerId, websites = [] } = props;

  const links = useMemo(() => {
    const validLinks: GameDetailsLink[] = [];

    if (igdbSlug) {
      validLinks.push({
        externalId: igdbId,
        name: 'IGDB',
        url: `https://www.igdb.com/games/${igdbSlug}`,
      });
    }

    websites
      .filter((website) =>
        ['Official', 'Steam', 'GOG', 'Epic', 'Itch', 'Wikipedia'].includes(
          website.type
        )
      )
      .forEach((website) => {
        validLinks.push({
          name: website.type,
          url: website.url,
        });
      });

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
  }, [igdbId, igdbSlug, youTubeTrailerId, websites]);

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
