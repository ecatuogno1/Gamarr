import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import LegendItem from './LegendItem';
import styles from './Legend.css';

function Legend() {
  const { showCutoffUnmetIcon, fullColorEvents } = useSelector(
    (state: AppState) => state.calendar.options
  );

  return (
    <div className={styles.legend}>
      <div>
        <LegendItem
          status="continuing"
          name={translate('Unreleased')}
          tooltip={translate('Unreleased')}
          fullColorEvents={fullColorEvents}
        />

        <LegendItem
          status="unmonitored"
          name={translate('Unmonitored')}
          tooltip={translate('DownloadedButNotMonitored')}
          fullColorEvents={fullColorEvents}
        />
      </div>

      <div>
        <LegendItem
          status="missingMonitored"
          name={translate('Missing')}
          tooltip={translate('MissingMonitoredAndConsideredAvailable')}
          fullColorEvents={fullColorEvents}
        />

        <LegendItem
          status="missingUnmonitored"
          name={translate('MissingNotMonitored')}
          tooltip={translate('MissingNotMonitored')}
          fullColorEvents={fullColorEvents}
        />
      </div>

      <div>
        <LegendItem
          status="queue"
          name={translate('Queued')}
          tooltip={translate('Queued')}
          fullColorEvents={fullColorEvents}
        />

        <LegendItem
          status="downloaded"
          name={translate('Downloaded')}
          tooltip={translate('DownloadedAndMonitored')}
          fullColorEvents={fullColorEvents}
        />
      </div>

      {showCutoffUnmetIcon ? (
        <div>
          <LegendItem
            name={translate('CutoffNotMet')}
            icon={icons.CUTOFF_NOT_MET}
            kind={kinds.WARNING}
            fullColorEvents={fullColorEvents}
            tooltip={translate('QualityCutoffNotMet')}
          />
        </div>
      ) : null}
    </div>
  );
}

export default Legend;
