import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { ReleaseProfilesAppState } from 'App/State/SettingsAppState';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { fetchIndexers } from 'Store/Actions/Settings/indexers';
import { fetchReleaseProfiles } from 'Store/Actions/Settings/releaseProfiles';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import translate from 'Utilities/String/translate';
import EditReleaseProfileModal from './EditReleaseProfileModal';
import ReleaseProfileItem from './ReleaseProfileItem';

function ReleaseProfiles() {
  const { items, isFetching, isPopulated, error }: ReleaseProfilesAppState =
    useSelector(createClientSideCollectionSelector('settings.releaseProfiles'));

  const tagList = useSelector(createTagsSelector());
  const indexerList = useSelector(
    (state: AppState) => state.settings.indexers.items
  );

  const dispatch = useDispatch();

  const [
    isAddReleaseProfileModalOpen,
    setAddReleaseProfileModalOpen,
    setAddReleaseProfileModalClosed,
  ] = useModalOpenState(false);

  useEffect(() => {
    dispatch(fetchReleaseProfiles());
    dispatch(fetchIndexers());
  }, [dispatch]);

  return (
    <PageSectionContent
      errorMessage={translate('ReleaseProfilesLoadError')}
      isFetching={isFetching}
      isPopulated={isPopulated}
      error={error}
    >
      <div className={settingsCardStyles.grid}>
        {items.map((item) => {
          return (
            <ReleaseProfileItem
              key={item.id}
              tagList={tagList}
              indexerList={indexerList}
              {...item}
            />
          );
        })}

        <AddCard
          label={translate('AddReleaseProfile')}
          onPress={setAddReleaseProfileModalOpen}
        />
      </div>

      <EditReleaseProfileModal
        isOpen={isAddReleaseProfileModalOpen}
        onModalClose={setAddReleaseProfileModalClosed}
      />
    </PageSectionContent>
  );
}

export default ReleaseProfiles;
