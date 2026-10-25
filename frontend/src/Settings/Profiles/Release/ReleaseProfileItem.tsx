import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import { Tag } from 'App/State/TagsAppState';
import Label from 'Components/Label';
import MiddleTruncate from 'Components/MiddleTruncate';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import TagList from 'Components/TagList';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons, kinds } from 'Helpers/Props';
import { deleteReleaseProfile } from 'Store/Actions/Settings/releaseProfiles';
import Indexer from 'typings/Indexer';
import ReleaseProfile from 'typings/Settings/ReleaseProfile';
import translate from 'Utilities/String/translate';
import EditReleaseProfileModal from './EditReleaseProfileModal';
import styles from './ReleaseProfileItem.css';

interface ReleaseProfileProps extends ReleaseProfile {
  tagList: Tag[];
  indexerList: Indexer[];
}

function ReleaseProfileItem(props: ReleaseProfileProps) {
  const {
    id,
    name,
    enabled = true,
    required = [],
    ignored = [],
    tags,
    indexerId = 0,
    tagList,
    indexerList,
  } = props;

  const dispatch = useDispatch();

  const [isEditModalOpen, setEditModalOpen, setEditModalClosed] =
    useModalOpenState(false);

  const [isDeleteModalOpen, setDeleteModalOpen, setDeleteModalClosed] =
    useModalOpenState(false);

  const handleConfirmDelete = useCallback(() => {
    dispatch(deleteReleaseProfile({ id }));
  }, [id, dispatch]);

  const indexer = indexerId
    ? indexerList.find((i) => i.id === indexerId)
    : undefined;

  const statusSegments = [translate(enabled ? 'Enabled' : 'Disabled')];

  return (
    <SettingsCard
      name={name || translate('Unnamed')}
      isUnnamed={!name}
      aria-label={translate('EditReleaseProfileName', { name: name ?? id })}
      actions={
        <SettingsCardAction
          title={translate('DeleteReleaseProfile')}
          aria-label={translate('DeleteReleaseProfile')}
          name={icons.DELETE}
          onPress={setDeleteModalOpen}
        />
      }
      onPress={setEditModalOpen}
    >
      <SettingsCardStatus
        dot={enabled ? 'active' : 'muted'}
        segments={statusSegments}
      />

      <div className={styles.chips}>
        {required.map((item) => {
          if (!item) return null;

          return (
            <Label
              key={item}
              className={settingsCardStyles.truncatedLabel}
              kind={kinds.SUCCESS}
            >
              <MiddleTruncate text={item} />
            </Label>
          );
        })}

        {ignored.map((item) => {
          if (!item) return null;

          return (
            <Label
              key={item}
              className={settingsCardStyles.truncatedLabel}
              kind={kinds.DANGER}
            >
              <MiddleTruncate text={item} />
            </Label>
          );
        })}
      </div>

      {tags.length ? <TagList tags={tags} tagList={tagList} /> : null}

      {indexer ? (
        <div className={settingsCardStyles.labels}>
          <Label kind={kinds.INFO} outline={true}>
            {indexer.name}
          </Label>
        </div>
      ) : null}

      <EditReleaseProfileModal
        id={id}
        isOpen={isEditModalOpen}
        onModalClose={setEditModalClosed}
        onDeleteReleaseProfilePress={setDeleteModalOpen}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteReleaseProfile')}
        message={translate('DeleteReleaseProfileMessageText', {
          name: name ?? id,
        })}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDelete}
        onCancel={setDeleteModalClosed}
      />
    </SettingsCard>
  );
}

export default ReleaseProfileItem;
