import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ImportGameTitle.css';

interface ImportGameTitleProps {
  title: string;
  year: number;
  platform?: string;
  isExistingGame: boolean;
}

function ImportGameTitle({
  title,
  year,
  platform,
  isExistingGame,
}: ImportGameTitleProps) {
  return (
    <div className={styles.titleContainer}>
      <div className={styles.title}>{title}</div>

      {year > 0 && !title.includes(String(year)) ? (
        <span className={styles.year}>({year})</span>
      ) : null}

      {platform ? <Label dot={false}>{platform}</Label> : null}

      {isExistingGame ? (
        <Label kind={kinds.WARNING}>{translate('Existing')}</Label>
      ) : null}
    </div>
  );
}

export default ImportGameTitle;
