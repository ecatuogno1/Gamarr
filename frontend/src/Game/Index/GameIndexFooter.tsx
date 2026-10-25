import React from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import { selectGameIndexTitles } from 'Game/Title/createGameTitleCollectionSelector';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './GameIndexFooter.css';

function createUnoptimizedSelector() {
  return createSelector(
    selectGameIndexTitles,
    (state: AppState) => state.games,
    (titles, games) => {
      // Statistics of the platform versions that match the filter
      const versions = titles.items
        .flatMap((title) => title.matchingGameIds)
        .map((id) => games.items[games.itemMap[id]])
        .filter((game) => !!game)
        .map((m) => {
          const { monitored, status, hasFile, statistics } = m;

          return {
            monitored,
            status,
            hasFile,
            statistics,
          };
        });

      return {
        titleCount: titles.items.length,
        versions,
      };
    }
  );
}

function createGameSelector() {
  return createDeepEqualSelector(createUnoptimizedSelector(), (games) => games);
}

export default function GameIndexFooter() {
  const { titleCount, versions: games } = useSelector(createGameSelector());
  const count = games.length;
  let gameFiles = 0;
  let monitored = 0;
  let totalFileSize = 0;

  games.forEach((m) => {
    const { statistics = { sizeOnDisk: 0 } } = m;

    const { sizeOnDisk = 0 } = statistics;

    if (m.hasFile) {
      gameFiles += 1;
    }

    if (m.monitored) {
      monitored++;
    }

    totalFileSize += sizeOnDisk;
  });

  return (
    <div className={styles.footer}>
      <div>
        <div className={styles.legendItem}>
          <div className={styles.ended} />
          <div>{translate('DownloadedAndMonitored')}</div>
        </div>

        <div className={styles.legendItem}>
          <div className={styles.availNotMonitored} />
          <div>{translate('DownloadedButNotMonitored')}</div>
        </div>

        <div className={styles.legendItem}>
          <div className={styles.missingMonitored} />
          <div>{translate('MissingMonitoredAndConsideredAvailable')}</div>
        </div>

        <div className={styles.legendItem}>
          <div className={styles.missingUnmonitored} />
          <div>{translate('MissingNotMonitored')}</div>
        </div>

        <div className={styles.legendItem}>
          <div className={styles.queue} />
          <div>{translate('Queued')}</div>
        </div>

        <div className={styles.legendItem}>
          <div className={styles.continuing} />
          <div>{translate('Unreleased')}</div>
        </div>
      </div>

      <div className={styles.statistics}>
        <DescriptionList>
          <DescriptionListItem title={translate('Titles')} data={titleCount} />

          <DescriptionListItem
            title={translate('PlatformVersions')}
            data={count}
          />
        </DescriptionList>

        <DescriptionList>
          <DescriptionListItem
            title={translate('GameFiles')}
            data={gameFiles}
          />

          <DescriptionListItem
            title={translate('Monitored')}
            data={monitored}
          />

          <DescriptionListItem
            title={translate('Unmonitored')}
            data={count - monitored}
          />
        </DescriptionList>

        <DescriptionList>
          <DescriptionListItem
            title={translate('TotalFileSize')}
            data={formatBytes(totalFileSize)}
          />
        </DescriptionList>
      </div>
    </div>
  );
}
