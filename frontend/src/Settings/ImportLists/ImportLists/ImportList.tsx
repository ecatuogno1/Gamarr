import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import TagList from 'Components/TagList';
import { icons, kinds } from 'Helpers/Props';
import { deleteImportList } from 'Store/Actions/settingsActions';
import useTags from 'Tags/useTags';
import formatShortTimeSpan from 'Utilities/Date/formatShortTimeSpan';
import translate from 'Utilities/String/translate';
import EditImportListModal from './EditImportListModal';

interface ImportListProps {
  id: number;
  name: string;
  enabled: boolean;
  enableAuto: boolean;
  tags: number[];
  minRefreshInterval: string;
  onCloneImportListPress: (id: number) => void;
}

function ImportList({
  id,
  name,
  enabled,
  enableAuto,
  tags,
  minRefreshInterval,
  onCloneImportListPress,
}: ImportListProps) {
  const dispatch = useDispatch();
  const tagList = useTags();

  const [isEditImportListModalOpen, setIsEditImportListModalOpen] =
    useState(false);

  const [isDeleteImportListModalOpen, setIsDeleteImportListModalOpen] =
    useState(false);

  const handleEditImportListPress = useCallback(() => {
    setIsEditImportListModalOpen(true);
  }, []);

  const handleEditImportListModalClose = useCallback(() => {
    setIsEditImportListModalOpen(false);
  }, []);

  const handleDeleteImportListPress = useCallback(() => {
    setIsEditImportListModalOpen(false);
    setIsDeleteImportListModalOpen(true);
  }, []);

  const handleDeleteImportListModalClose = useCallback(() => {
    setIsDeleteImportListModalOpen(false);
  }, []);

  const handleConfirmDeleteImportList = useCallback(() => {
    dispatch(deleteImportList({ id }));
  }, [id, dispatch]);

  const handleCloneImportListPress = useCallback(() => {
    onCloneImportListPress(id);
  }, [id, onCloneImportListPress]);

  return (
    <SettingsCard
      name={name}
      aria-label={translate('EditImportListName', { name })}
      actions={
        <SettingsCardAction
          title={translate('CloneImportList')}
          aria-label={translate('CloneImportList')}
          name={icons.CLONE}
          onPress={handleCloneImportListPress}
        />
      }
      onPress={handleEditImportListPress}
    >
      <SettingsCardStatus
        dot={enabled ? 'active' : 'muted'}
        segments={[
          translate(enabled ? 'Enabled' : 'Disabled'),
          enableAuto ? translate('AutomaticAdd') : null,
          <>
            {translate('Refresh')}: {formatShortTimeSpan(minRefreshInterval)}
          </>,
        ]}
      />

      {tags.length > 0 ? <TagList tags={tags} tagList={tagList} /> : null}

      <EditImportListModal
        id={id}
        isOpen={isEditImportListModalOpen}
        onModalClose={handleEditImportListModalClose}
        onDeleteImportListPress={handleDeleteImportListPress}
      />

      <ConfirmModal
        isOpen={isDeleteImportListModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteImportList')}
        message={translate('DeleteImportListMessageText', { name })}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteImportList}
        onCancel={handleDeleteImportListModalClose}
      />
    </SettingsCard>
  );
}

export default ImportList;
