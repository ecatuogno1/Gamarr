import React from 'react';
import Tooltip from 'Components/Tooltip/Tooltip';
import { Ratings } from 'Game/Game';
import { kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './IgdbRating.css';

interface IgdbRatingProps {
  ratings: Ratings;
  iconSize?: number;
  hideIcon?: boolean;
}

// IGDB user rating, 0-100
function IgdbRating(props: IgdbRatingProps) {
  const { ratings, iconSize = 14, hideIcon = false } = props;

  const { value = 0, votes = 0 } = ratings.igdb ?? {};

  return (
    <Tooltip
      anchor={
        <span className={styles.wrapper}>
          {!hideIcon && (
            <span
              className={styles.label}
              style={{
                fontSize: `${iconSize - 2}px`,
              }}
            >
              IGDB
            </span>
          )}
          {value.toFixed()}%
        </span>
      }
      tooltip={translate('CountVotes', { votes })}
      kind={kinds.INVERSE}
      position={tooltipPositions.TOP}
    />
  );
}

export default IgdbRating;
