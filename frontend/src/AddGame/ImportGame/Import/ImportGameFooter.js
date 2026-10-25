import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import AvailabilitySelectInput from 'Components/Form/Select/AvailabilitySelectInput';
import MonitorGamesSelectInput from 'Components/Form/Select/MonitorGamesSelectInput';
import QualityProfileSelectInput from 'Components/Form/Select/QualityProfileSelectInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ImportGameFooter.css';

const MIXED = 'mixed';

class ImportGameFooter extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      defaultMonitor,
      defaultQualityProfileId,
      defaultMinimumAvailability
    } = props;

    this.state = {
      monitor: defaultMonitor,
      qualityProfileId: defaultQualityProfileId,
      minimumAvailability: defaultMinimumAvailability
    };
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      defaultMonitor,
      defaultQualityProfileId,
      defaultMinimumAvailability,
      isMonitorMixed,
      isQualityProfileIdMixed,
      isMinimumAvailabilityMixed
    } = this.props;

    const {
      monitor,
      qualityProfileId,
      minimumAvailability
    } = this.state;

    const newState = {};

    if (isMonitorMixed && monitor !== MIXED) {
      newState.monitor = MIXED;
    } else if (!isMonitorMixed && monitor !== defaultMonitor) {
      newState.monitor = defaultMonitor;
    }

    if (isQualityProfileIdMixed && qualityProfileId !== MIXED) {
      newState.qualityProfileId = MIXED;
    } else if (!isQualityProfileIdMixed && qualityProfileId !== defaultQualityProfileId) {
      newState.qualityProfileId = defaultQualityProfileId;
    }

    if (isMinimumAvailabilityMixed && minimumAvailability !== MIXED) {
      newState.minimumAvailability = MIXED;
    } else if (!isMinimumAvailabilityMixed && minimumAvailability !== defaultMinimumAvailability) {
      newState.minimumAvailability = defaultMinimumAvailability;
    }

    if (!_.isEmpty(newState)) {
      this.setState(newState);
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });
    this.props.onInputChange({ name, value });
  };

  //
  // Render

  render() {
    const {
      selectedCount,
      totalCount,
      readyCount,
      existingCount,
      unsearchedCount,
      allSelected,
      allUnselected,
      isImporting,
      isLookingUpGame,
      isMonitorMixed,
      isQualityProfileIdMixed,
      isMinimumAvailabilityMixed,
      hasUnsearchedItems,
      importError,
      onSelectAllChange,
      onImportPress,
      onLookupPress,
      onCancelLookupPress
    } = this.props;

    const {
      monitor,
      qualityProfileId,
      minimumAvailability
    } = this.state;

    const needsAttentionCount = totalCount - readyCount - existingCount;

    let selectAllValue = null;

    if (allUnselected || !selectedCount) {
      selectAllValue = false;
    } else if (allSelected) {
      selectAllValue = true;
    }

    return (
      <PageContentFooter className={styles.footerShell}>
        <div className={styles.defaultsBar}>
          <div className={styles.defaultsContent}>
            <div className={styles.defaultsIdentity}>
              <div className={styles.defaultsTitle}>
                {translate('Defaults')}
              </div>
            </div>

            <div className={styles.setting}>
              <div className={styles.label}>
                {translate('Monitor')}
              </div>

              <MonitorGamesSelectInput
                name="monitor"
                value={monitor}
                modalTitle={translate('Monitor')}
                isDisabled={!selectedCount}
                includeMixed={isMonitorMixed}
                onChange={this.onInputChange}
              />
            </div>

            <div className={styles.setting}>
              <div className={styles.label}>
                {translate('MinimumAvailability')}
              </div>

              <AvailabilitySelectInput
                name="minimumAvailability"
                value={minimumAvailability}
                modalTitle={translate('MinimumAvailability')}
                isDisabled={!selectedCount}
                includeMixed={isMinimumAvailabilityMixed}
                onChange={this.onInputChange}
              />
            </div>

            <div className={styles.setting}>
              <div className={styles.label}>
                {translate('QualityProfile')}
              </div>

              <QualityProfileSelectInput
                name="qualityProfileId"
                value={qualityProfileId}
                modalTitle={translate('QualityProfile')}
                isDisabled={!selectedCount}
                includeMixed={isQualityProfileIdMixed}
                onChange={this.onInputChange}
              />
            </div>
          </div>
        </div>

        <div className={styles.footerContent}>
          <div className={styles.footerRow}>
            <div className={styles.selectionGroup}>
              <CheckInput
                className={styles.selectAllInput}
                containerClassName={styles.selectAllContainer}
                name="selectAllRows"
                ariaLabel={translate('SelectAll')}
                value={selectAllValue}
                onChange={onSelectAllChange}
              />

              <span className={styles.selectionLabel}>
                {translate('CountOfTotalSelected', {
                  selectedCount,
                  totalCount
                })}
              </span>
            </div>

            <div className={styles.queueStatus}>
              <span className={styles.statusItem}>
                <span className={styles.readyDot} />
                <strong>{translate('CountReady', { count: readyCount })}</strong>
              </span>

              {
                needsAttentionCount > 0 ?
                  <span className={styles.statusItem}>
                    <span className={styles.attentionDot} />
                    {translate('CountPending', { count: needsAttentionCount })}
                  </span> :
                  null
              }

              {
                existingCount ?
                  <span className={styles.statusItem}>
                    <span className={styles.existingDot} />
                    {translate('CountAlreadyInLibrary', { count: existingCount })}
                  </span> :
                  null
              }
            </div>

            <div className={styles.buttonGroup}>
              <div className={styles.importButtonContainer}>
                <SpinnerButton
                  className={styles.importButton}
                  kind={kinds.DEFAULT}
                  isSpinning={isImporting}
                  isDisabled={!selectedCount || isLookingUpGame}
                  onPress={onImportPress}
                >
                  {
                    selectedCount === 1 ?
                      translate('ImportCountGame', { selectedCount }) :
                      translate('ImportCountGames', { selectedCount })
                  }
                </SpinnerButton>

                {
                  isLookingUpGame ?
                    <Button
                      className={styles.stopButton}
                      kind={kinds.PRIMARY}
                      onPress={onCancelLookupPress}
                    >
                      <Icon
                        name={icons.SPINNER}
                        isSpinning={true}
                        size={14}
                      />

                      {translate('Stop')}
                    </Button> :
                    null
                }

                {
                  hasUnsearchedItems ?
                    <Button
                      className={styles.loadingButton}
                      kind={kinds.PRIMARY}
                      onPress={onLookupPress}
                    >
                      {
                        unsearchedCount === 1 ?
                          translate('SearchCountFolder', { count: unsearchedCount }) :
                          translate('SearchCountFolders', { count: unsearchedCount })
                      }
                    </Button> :
                    null
                }

                {
                  importError ?
                    <Popover
                      anchor={
                        <Icon
                          className={styles.importError}
                          name={icons.WARNING}
                          kind={kinds.WARNING}
                        />
                      }
                      title={translate('ImportErrors')}
                      body={
                        <ul>
                          {
                            Array.isArray(importError.responseJSON) ?
                              importError.responseJSON.map((error, index) => {
                                return (
                                  <li key={index}>
                                    {error.errorMessage}
                                  </li>
                                );
                              }) :
                              <li>
                                {
                                  JSON.stringify(importError.responseJSON)
                                }
                              </li>
                          }
                        </ul>
                      }
                      position={tooltipPositions.RIGHT}
                    /> :
                    null
                }
              </div>
            </div>
          </div>
        </div>
      </PageContentFooter>
    );
  }
}

ImportGameFooter.propTypes = {
  selectedCount: PropTypes.number.isRequired,
  totalCount: PropTypes.number.isRequired,
  readyCount: PropTypes.number.isRequired,
  existingCount: PropTypes.number.isRequired,
  unsearchedCount: PropTypes.number.isRequired,
  allSelected: PropTypes.bool.isRequired,
  allUnselected: PropTypes.bool.isRequired,
  isImporting: PropTypes.bool.isRequired,
  isLookingUpGame: PropTypes.bool.isRequired,
  defaultMonitor: PropTypes.string.isRequired,
  defaultQualityProfileId: PropTypes.number,
  defaultMinimumAvailability: PropTypes.string,
  isMonitorMixed: PropTypes.bool.isRequired,
  isQualityProfileIdMixed: PropTypes.bool.isRequired,
  isMinimumAvailabilityMixed: PropTypes.bool.isRequired,
  hasUnsearchedItems: PropTypes.bool.isRequired,
  importError: PropTypes.object,
  onInputChange: PropTypes.func.isRequired,
  onSelectAllChange: PropTypes.func.isRequired,
  onImportPress: PropTypes.func.isRequired,
  onLookupPress: PropTypes.func.isRequired,
  onCancelLookupPress: PropTypes.func.isRequired
};

export default ImportGameFooter;
