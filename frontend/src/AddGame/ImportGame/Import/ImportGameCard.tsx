import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import CheckInput from 'Components/Form/CheckInput';
import AvailabilitySelectInput from 'Components/Form/Select/AvailabilitySelectInput';
import MonitorGamesSelectInput from 'Components/Form/Select/MonitorGamesSelectInput';
import QualityProfileSelectInput from 'Components/Form/Select/QualityProfileSelectInput';
import Label from 'Components/Label';
import GamePoster from 'Game/GamePoster';
import { kinds } from 'Helpers/Props';
import { setImportGameValue } from 'Store/Actions/importGameActions';
import { CheckInputChanged, InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { ImportGameEligibility, ImportGameItem } from './importGameTypes';
import ImportGameSelectGame from './SelectGame/ImportGameSelectGame';
import styles from './ImportGameCard.css';

function getMatchStatusLabel(
  eligibility: ImportGameEligibility,
  item: ImportGameItem
) {
  switch (eligibility) {
    case 'existing':
      return translate('AlreadyInLibrary');
    case 'unmatched':
      return item.isPopulated
        ? translate('NoMatchFound')
        : translate('Pending');
    case 'ready':
    default:
      return null;
  }
}

interface ImportGameCardProps {
  item: ImportGameItem;
  eligibility: ImportGameEligibility;
  isSelected: boolean;
  isStacked: boolean;
  onSelectedChange: (change: CheckInputChanged & { id: string }) => void;
}

function ImportGameCard({
  item,
  eligibility,
  isSelected,
  isStacked,
  onSelectedChange,
}: ImportGameCardProps) {
  const dispatch = useDispatch();

  const {
    id,
    path,
    relativePath,
    monitor,
    qualityProfileId,
    minimumAvailability,
    selectedGame,
  } = item;

  const [isSearching, setIsSearching] = useState(false);

  const needsAttention = eligibility !== 'ready';
  const matchStatusLabel = getMatchStatusLabel(eligibility, item);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error actions aren't typed
      dispatch(setImportGameValue({ id, [name]: value }));
    },
    [id, dispatch]
  );

  const handleSelectedChange = useCallback(
    (change: CheckInputChanged) => {
      onSelectedChange({ ...change, id });
    },
    [id, onSelectedChange]
  );

  return (
    <div
      className={classNames(
        styles.rowLayout,
        isStacked && styles.stacked,
        needsAttention && styles.needsAttention,
        selectedGame && styles.matched,
        isSearching && styles.searching
      )}
    >
      <CheckInput
        className={styles.selectInput}
        containerClassName={styles.selectContainer}
        name={id}
        ariaLabel={translate('SelectRow')}
        value={isSelected}
        isDisabled={needsAttention}
        onChange={handleSelectedChange}
      />

      <div className={styles.poster}>
        {selectedGame ? (
          <GamePoster
            images={selectedGame.images}
            size={250}
            lazy={false}
            overflow={true}
          />
        ) : (
          <div className={styles.posterPlaceholder}>?</div>
        )}
      </div>

      <div
        className={classNames(
          styles.gameInfo,
          needsAttention && styles.attentionInfo
        )}
      >
        <div className={styles.identity}>
          {selectedGame ? (
            <>
              <div className={styles.titleRow}>
                <ImportGameSelectGame
                  id={id}
                  onEditingChange={setIsSearching}
                />
              </div>

              <div className={styles.source}>
                <span className={styles.folder}>{relativePath}</span>
                {selectedGame.platform
                  ? ` · ${selectedGame.platform.shortName}`
                  : null}
              </div>
            </>
          ) : (
            <>
              <div className={styles.folderName}>{relativePath}</div>
              <div className={styles.source}>
                <span className={styles.folder}>{path}</span>
              </div>
            </>
          )}
        </div>

        {needsAttention && matchStatusLabel ? (
          <>
            <Label
              className={classNames(
                styles.matchStatus,
                !selectedGame && styles.matchStatusStackedHidden
              )}
              kind={kinds.WARNING}
            >
              {matchStatusLabel}
            </Label>

            {selectedGame ? null : (
              <div className={styles.matchAction}>
                <ImportGameSelectGame
                  id={id}
                  onEditingChange={setIsSearching}
                />
              </div>
            )}
          </>
        ) : (
          <div className={styles.settingsRow}>
            <div className={styles.setting}>
              <div className={styles.settingLabel}>{translate('Monitor')}</div>

              <MonitorGamesSelectInput
                name="monitor"
                value={monitor ?? 'gameOnly'}
                modalTitle={translate('Monitor')}
                isDisabled={!selectedGame}
                onChange={handleInputChange}
              />
            </div>

            <div className={styles.setting}>
              <div className={styles.settingLabel}>
                {translate('MinimumAvailability')}
              </div>

              <AvailabilitySelectInput
                name="minimumAvailability"
                value={minimumAvailability ?? 'released'}
                modalTitle={translate('MinimumAvailability')}
                isDisabled={!selectedGame}
                onChange={handleInputChange}
              />
            </div>

            <div className={styles.setting}>
              <div className={styles.settingLabel}>
                {translate('QualityProfile')}
              </div>

              <QualityProfileSelectInput
                name="qualityProfileId"
                value={qualityProfileId ?? 0}
                modalTitle={translate('QualityProfile')}
                isDisabled={!selectedGame}
                onChange={handleInputChange}
              />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default ImportGameCard;
