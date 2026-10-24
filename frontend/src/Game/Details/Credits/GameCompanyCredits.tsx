import React from 'react';
import { useSelector } from 'react-redux';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import createGameCreditsSelector from 'Store/Selectors/createGameCreditsSelector';
import { GameCreditType } from 'typings/GameCredit';
import translate from 'Utilities/String/translate';
import styles from './GameCompanyCredits.css';

const creditTypes: GameCreditType[] = [
  'developer',
  'publisher',
  'porting',
  'supporting',
];

function getCreditTypeLabel(type: GameCreditType) {
  switch (type) {
    case 'developer':
      return translate('Developer');
    case 'publisher':
      return translate('Publisher');
    case 'porting':
      return translate('Porting');
    default:
      return translate('Supporting');
  }
}

// Company credits (developers, publishers, porting and supporting studios) from IGDB
function GameCompanyCredits() {
  const { items } = useSelector(createGameCreditsSelector(creditTypes));

  if (!items.length) {
    return <div>{translate('NoCompanyCredits')}</div>;
  }

  // Only reserve room for logos when IGDB has any, most companies don't have one
  const hasLogos = items.some((credit) =>
    credit.images.some((image) => image.coverType === 'clearlogo')
  );

  return (
    <div className={styles.credits}>
      {items.map((credit) => {
        const logo = credit.images.find(
          (image) => image.coverType === 'clearlogo'
        );

        return (
          <div key={credit.id} className={styles.credit}>
            {hasLogos ? (
              <div className={styles.logoContainer}>
                {logo ? (
                  <img
                    className={styles.logo}
                    src={logo.remoteUrl}
                    alt={credit.companyName}
                    loading="lazy"
                  />
                ) : null}
              </div>
            ) : null}

            <div className={styles.name}>{credit.companyName}</div>

            <Label kind={kinds.INFO}>{getCreditTypeLabel(credit.type)}</Label>
          </div>
        );
      })}
    </div>
  );
}

export default GameCompanyCredits;
