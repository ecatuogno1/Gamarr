import React from 'react';
import Tooltip from 'Components/Tooltip/Tooltip';
import { Ratings } from 'Game/Game';
import { kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './CriticRating.css';

interface CriticRatingProps {
  ratings: Ratings;
  iconSize?: number;
  hideIcon?: boolean;
}

// IGDB aggregated critic rating, 0-100
function CriticRating(props: CriticRatingProps) {
  const { ratings, iconSize = 14, hideIcon = false } = props;

  const { value = 0, votes = 0 } = ratings.critic ?? {};

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
              {translate('Critic')}
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

export default CriticRating;
