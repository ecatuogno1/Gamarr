import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { IndexerAppState } from 'App/State/SettingsAppState';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import { cloneIndexer, fetchIndexers } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import IndexerModel from 'typings/Indexer';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import AddIndexerModal from './AddIndexerModal';
import EditIndexerModal from './EditIndexerModal';
import Indexer from './Indexer';

function Indexers() {
  const dispatch = useDispatch();

  const { isFetching, isPopulated, items, error } = useSelector(
    createSortedSectionSelector<IndexerModel, IndexerAppState>(
      'settings.indexers',
      sortByProp('name')
    )
  );

  const [isAddIndexerModalOpen, setIsAddIndexerModalOpen] = useState(false);
  const [isEditIndexerModalOpen, setIsEditIndexerModalOpen] = useState(false);

  const showPriority = items.some((index) => index.priority !== 25);

  const handleAddIndexerPress = useCallback(() => {
    setIsAddIndexerModalOpen(true);
  }, []);

  const handleCloneIndexerPress = useCallback(
    (id: number) => {
      dispatch(cloneIndexer({ id }));
      setIsEditIndexerModalOpen(true);
    },
    [dispatch]
  );

  const handleIndexerSelect = useCallback(() => {
    setIsAddIndexerModalOpen(false);
    setIsEditIndexerModalOpen(true);
  }, []);

  const handleAddIndexerModalClose = useCallback(() => {
    setIsAddIndexerModalOpen(false);
  }, []);

  const handleEditIndexerModalClose = useCallback(() => {
    setIsEditIndexerModalOpen(false);
  }, []);

  useEffect(() => {
    dispatch(fetchIndexers());
  }, [dispatch]);

  return (
    <PageSectionContent
      errorMessage={translate('IndexersLoadError')}
      error={error}
      isFetching={isFetching}
      isPopulated={isPopulated}
    >
      <div className={settingsCardStyles.grid}>
        {items.map((item) => {
          return (
            <Indexer
              key={item.id}
              {...item}
              showPriority={showPriority}
              onCloneIndexerPress={handleCloneIndexerPress}
            />
          );
        })}

        <AddCard
          label={translate('AddIndexer')}
          onPress={handleAddIndexerPress}
        />
      </div>

      <AddIndexerModal
        isOpen={isAddIndexerModalOpen}
        onIndexerSelect={handleIndexerSelect}
        onModalClose={handleAddIndexerModalClose}
      />

      <EditIndexerModal
        isOpen={isEditIndexerModalOpen}
        onModalClose={handleEditIndexerModalClose}
      />
    </PageSectionContent>
  );
}

export default Indexers;
