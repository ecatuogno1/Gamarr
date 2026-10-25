import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { useCallback, useMemo, useState } from 'react';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus, { SettingsCardStatusValue } from 'Components/SettingsCard/SettingsCardStatus';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditQualityProfileModalConnector from './EditQualityProfileModalConnector';
import styles from './QualityProfile.css';

const CHIP_CAP_THRESHOLD = 6;

function QualityProfile(props) {
  const {
    id,
    name,
    upgradeAllowed,
    cutoff,
    items,
    isDeleting,
    onConfirmDeleteQualityProfile,
    onCloneQualityProfilePress
  } = props;

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  const allowedItems = useMemo(() => items.filter((item) => item.allowed), [items]);

  const cutoffName = useMemo(() => {
    for (const item of items) {
      if (item.allowed) {
        if (item.quality) {
          if (item.quality.id === cutoff) {
            return item.quality.name;
          }
        } else if (item.id === cutoff) {
          return item.name;
        }
      }
    }

    return '';
  }, [items, cutoff]);

  const isCapped = !isExpanded && allowedItems.length > CHIP_CAP_THRESHOLD;
  const visibleItems = isCapped ? allowedItems.slice(0, CHIP_CAP_THRESHOLD) : allowedItems;
  const hiddenCount = allowedItems.length - visibleItems.length;

  const handleEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, []);

  const handleEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, []);

  const handleDeletePress = useCallback(() => {
    setIsEditModalOpen(false);
    setIsDeleteModalOpen(true);
  }, []);

  const handleDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  const handleConfirmDelete = useCallback(() => {
    onConfirmDeleteQualityProfile(id);
  }, [id, onConfirmDeleteQualityProfile]);

  const handleClonePress = useCallback(() => {
    onCloneQualityProfilePress(id);
  }, [id, onCloneQualityProfilePress]);

  const handleShowAllPress = useCallback((e) => {
    e.stopPropagation();
    setIsExpanded(true);
  }, []);

  const upgradeTitle = translate('UpgradeUntilThisQualityIsMetOrExceeded');

  return (
    <SettingsCard
      name={name}
      aria-label={translate('EditQualityProfileName', { name })}
      actions={
        <SettingsCardAction
          title={translate('CloneProfile')}
          aria-label={translate('CloneProfile')}
          name={icons.CLONE}
          onPress={handleClonePress}
        />
      }
      onPress={handleEditPress}
    >
      <SettingsCardStatus
        dot={upgradeAllowed ? 'active' : undefined}
        segments={[
          translate(upgradeAllowed ? 'UpgradesOn' : 'UpgradesOff'),
          cutoffName ? (
            <>
              {translate('Cutoff')}{' '}
              <SettingsCardStatusValue>{cutoffName}</SettingsCardStatusValue>
            </>
          ) : null
        ]}
      />

      <div className={styles.chips}>
        {
          visibleItems.map((item) => {
            if (item.quality) {
              const isCutoff = item.quality.id === cutoff;

              return (
                <Label
                  key={item.quality.id}
                  className={isCutoff ? styles.cutoffChip : undefined}
                  outline={!isCutoff}
                  title={isCutoff && upgradeAllowed ? upgradeTitle : undefined}
                >
                  {item.quality.name}
                </Label>
              );
            }

            const isCutoff = item.id === cutoff;

            return (
              <Tooltip
                key={item.id}
                anchor={
                  <Label
                    className={classNames(
                      styles.groupChip,
                      isCutoff && styles.cutoffChip
                    )}
                    outline={!isCutoff}
                    title={isCutoff && upgradeAllowed ? upgradeTitle : undefined}
                  >
                    <span className={styles.groupName}>{item.name}</span>

                    <span className={styles.groupCount}>
                      +{item.items.length}
                    </span>
                  </Label>
                }
                tooltip={
                  <div>
                    {
                      item.items.map((groupItem) => (
                        <Label key={groupItem.quality.id} kind={kinds.DEFAULT}>
                          {groupItem.quality.name}
                        </Label>
                      ))
                    }
                  </div>
                }
                kind={kinds.INVERSE}
                position={tooltipPositions.TOP}
              />
            );
          })
        }

        {
          hiddenCount > 0 ?
            <button
              className={styles.moreChip}
              type="button"
              onClick={handleShowAllPress}
            >
              {translate('ShowMoreCount', { count: hiddenCount })}
            </button> :
            null
        }
      </div>

      <EditQualityProfileModalConnector
        id={id}
        isOpen={isEditModalOpen}
        onModalClose={handleEditModalClose}
        onDeleteQualityProfilePress={handleDeletePress}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteQualityProfile')}
        message={translate('DeleteQualityProfileMessageText', { name })}
        confirmLabel={translate('Delete')}
        isSpinning={isDeleting}
        onConfirm={handleConfirmDelete}
        onCancel={handleDeleteModalClose}
      />
    </SettingsCard>
  );
}

QualityProfile.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  upgradeAllowed: PropTypes.bool.isRequired,
  cutoff: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDeleting: PropTypes.bool.isRequired,
  onConfirmDeleteQualityProfile: PropTypes.func.isRequired,
  onCloneQualityProfilePress: PropTypes.func.isRequired
};

export default QualityProfile;
