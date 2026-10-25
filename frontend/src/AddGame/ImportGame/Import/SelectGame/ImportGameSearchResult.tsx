import classNames from 'classnames';
import React, { useCallback, useEffect, useRef } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import ImportGameTitle from './ImportGameTitle';
import styles from './ImportGameSearchResult.css';

interface ImportGameSearchResultProps {
  id: string;
  igdbId: number;
  title: string;
  year: number;
  platform?: string;
  isExistingGame: boolean;
  isHighlighted: boolean;
  onPress: (igdbId: number) => void;
}

function ImportGameSearchResult({
  id,
  igdbId,
  title,
  year,
  platform,
  isExistingGame,
  isHighlighted,
  onPress,
}: ImportGameSearchResultProps) {
  const rowRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (isHighlighted) {
      rowRef.current?.scrollIntoView({ block: 'nearest' });
    }
  }, [isHighlighted]);

  const handlePress = useCallback(() => {
    onPress(igdbId);
  }, [igdbId, onPress]);

  return (
    <div
      ref={rowRef}
      className={classNames(
        styles.container,
        isHighlighted ? styles.highlighted : undefined
      )}
      role="row"
      id={id}
      aria-selected={isHighlighted}
    >
      <div className={styles.gameCell} role="gridcell">
        <Link className={styles.game} onPress={handlePress}>
          <ImportGameTitle
            title={title}
            year={year}
            platform={platform}
            isExistingGame={isExistingGame}
          />
        </Link>
      </div>

      <div className={styles.igdbLinkCell} role="gridcell">
        <Link
          className={styles.igdbLink}
          to={`https://www.thegamedb.org/game/${igdbId}`}
        >
          <Icon
            className={styles.igdbLinkIcon}
            name={icons.EXTERNAL_LINK}
            size={16}
          />
        </Link>
      </div>
    </div>
  );
}

export default ImportGameSearchResult;
