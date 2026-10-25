import React from 'react';
import Link from 'Components/Link/Link';
import translate from 'Utilities/String/translate';
import styles from './MetadataAttribution.css';

// Sonarr v5 credits TheTVDB under the details hero; Gamarr's metadata comes
// from IGDB, which requires attribution as well.
export default function MetadataAttribution() {
  return (
    <div className={styles.container}>
      <Link className={styles.attribution} to="https://www.igdb.com/">
        {translate('MetadataProvidedBy', { provider: 'IGDB' })}
      </Link>
    </div>
  );
}
