import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import MetadataAppState from 'App/State/MetadataAppState';
import PageSectionContent from 'Components/Page/PageSectionContent';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import { fetchMetadata } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import MetadataType from 'typings/Metadata';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import Metadata from './Metadata';

function createMetadatasSelector() {
  return createSelector(
    createSortedSectionSelector<MetadataType, MetadataAppState>(
      'settings.metadata',
      sortByProp('name')
    ),
    (metadata: MetadataAppState) => metadata
  );
}

function Metadatas() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, error, items } = useSelector(
    createMetadatasSelector()
  );

  useEffect(() => {
    dispatch(fetchMetadata());
  }, [dispatch]);

  return (
    <PageSectionContent
      error={error}
      errorMessage={translate('MetadataLoadError')}
      isFetching={isFetching}
      isPopulated={isPopulated}
    >
      <div className={settingsCardStyles.grid}>
        {items.map((item) => {
          return <Metadata key={item.id} {...item} />;
        })}
      </div>
    </PageSectionContent>
  );
}

export default Metadatas;
