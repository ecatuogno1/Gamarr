import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { ImportListAppState } from 'App/State/SettingsAppState';
import PageSectionContent from 'Components/Page/PageSectionContent';
import SectionHeading from 'Components/SectionHeading';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import settingsStyles from 'Settings/Settings.css';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import {
  cloneImportList,
  fetchImportLists,
} from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import ImportListModel from 'typings/ImportList';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import AddImportListModal from './AddImportListModal';
import EditImportListModal from './EditImportListModal';
import ImportList from './ImportList';

function ImportLists() {
  const dispatch = useDispatch();

  const { isFetching, isPopulated, items, error } = useSelector(
    createSortedSectionSelector<ImportListModel, ImportListAppState>(
      'settings.importLists',
      sortByProp('name')
    )
  );

  const [isAddImportListModalOpen, setIsAddImportListModalOpen] =
    useState(false);
  const [isEditImportListModalOpen, setIsEditImportListModalOpen] =
    useState(false);

  const handleAddImportListPress = useCallback(() => {
    setIsAddImportListModalOpen(true);
  }, []);

  const handleAddImportListModalClose = useCallback(() => {
    setIsAddImportListModalOpen(false);
  }, []);

  const handleImportListSelect = useCallback(() => {
    setIsAddImportListModalOpen(false);
    setIsEditImportListModalOpen(true);
  }, []);

  const handleEditImportListModalClose = useCallback(() => {
    setIsEditImportListModalOpen(false);
  }, []);

  const handleCloneImportListPress = useCallback(
    (id: number) => {
      dispatch(cloneImportList({ id }));
      setIsEditImportListModalOpen(true);
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchImportLists());
    dispatch(fetchRootFolders());
  }, [dispatch]);

  return (
    <div className={settingsStyles.pageSection}>
      <SectionHeading
        title={translate('ImportLists')}
        description={translate('ImportListsSectionDescription')}
      />

      <PageSectionContent
        errorMessage={translate('ImportListsLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isPopulated}
      >
        <div className={settingsCardStyles.grid}>
          {items.map((item) => {
            return (
              <ImportList
                key={item.id}
                {...item}
                onCloneImportListPress={handleCloneImportListPress}
              />
            );
          })}

          <AddCard
            label={translate('AddImportList')}
            onPress={handleAddImportListPress}
          />
        </div>

        <AddImportListModal
          isOpen={isAddImportListModalOpen}
          onImportListSelect={handleImportListSelect}
          onModalClose={handleAddImportListModalClose}
        />

        <EditImportListModal
          isOpen={isEditImportListModalOpen}
          onModalClose={handleEditImportListModalClose}
        />
      </PageSectionContent>
    </div>
  );
}

export default ImportLists;
