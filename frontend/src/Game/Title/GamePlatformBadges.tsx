import classNames from 'classnames';
import React from 'react';
import Link from 'Components/Link/Link';
import { GameTitleVersion, getTitleLink } from './gameTitle';
import styles from './GamePlatformBadges.css';

interface GamePlatformBadgesProps {
  className?: string;
  versions: GameTitleVersion[];
  size?: 'small' | 'large';
  selectedGameId?: number;
  // Badges link to the title page with their platform selected, unless onSelect is given
  onSelect?: (gameId: number) => void;
}

function GamePlatformBadges({
  className,
  versions,
  size = 'small',
  selectedGameId,
  onSelect,
}: GamePlatformBadgesProps) {
  return (
    <div className={classNames(styles.badges, className)}>
      {versions.map(({ game, kind, tooltip }) => {
        const linkProps = onSelect
          ? { onPress: () => onSelect(game.id) }
          : { to: getTitleLink(game.igdbId, game.platformId) };

        return (
          <Link
            key={game.id}
            className={classNames(
              styles.badge,
              styles[kind as keyof typeof styles],
              size === 'large' && styles.large,
              selectedGameId === game.id && styles.selected
            )}
            title={tooltip}
            {...linkProps}
          >
            {game.platform?.shortName ?? game.platformId}
          </Link>
        );
      })}
    </div>
  );
}

export default GamePlatformBadges;
