import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { AutoTaggingAppState } from 'App/State/SettingsAppState';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import {
  cloneAutoTagging,
  deleteAutoTagging,
  fetchAutoTaggings,
} from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import AutoTaggingModel from 'typings/AutoTagging';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import AutoTagging from './AutoTagging';
import EditAutoTaggingModal from './EditAutoTaggingModal';

export default function AutoTaggings() {
  const { error, items, isDeleting, isFetching, isPopulated } = useSelector(
    createSortedSectionSelector<AutoTaggingModel, AutoTaggingAppState>(
      'settings.autoTaggings',
      sortByProp('name')
    )
  );

  const tagList = useSelector(createTagsSelector());
  const dispatch = useDispatch();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [tagsFromId, setTagsFromId] = useState<number>();

  const onClonePress = useCallback(
    (id: number) => {
      dispatch(cloneAutoTagging({ id }));

      setTagsFromId(id);
      setIsEditModalOpen(true);
    },
    [dispatch, setIsEditModalOpen]
  );

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const onConfirmDelete = useCallback(
    (id: number) => {
      dispatch(deleteAutoTagging({ id }));
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchAutoTaggings());
    dispatch(fetchRootFolders());
  }, [dispatch]);

  return (
    <PageSectionContent
      errorMessage={translate('AutoTaggingLoadError')}
      error={error}
      isFetching={isFetching}
      isPopulated={isPopulated}
    >
      <div className={settingsCardStyles.grid}>
        {items.map((item) => {
          return (
            <AutoTagging
              key={item.id}
              {...item}
              isDeleting={isDeleting}
              tagList={tagList}
              onConfirmDeleteAutoTagging={onConfirmDelete}
              onCloneAutoTaggingPress={onClonePress}
            />
          );
        })}

        <AddCard label={translate('AddAutoTag')} onPress={onEditPress} />
      </div>

      <EditAutoTaggingModal
        isOpen={isEditModalOpen}
        tagsFromId={tagsFromId}
        onModalClose={onEditModalClose}
      />
    </PageSectionContent>
  );
}
