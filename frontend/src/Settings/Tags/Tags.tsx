import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import TagsAppState, { Tag as TagModel } from 'App/State/TagsAppState';
import Alert from 'Components/Alert';
import PageSectionContent from 'Components/Page/PageSectionContent';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import { kinds } from 'Helpers/Props';
import {
  fetchDelayProfiles,
  fetchDownloadClients,
  fetchImportLists,
  fetchIndexers,
  fetchNotifications,
  fetchReleaseProfiles,
} from 'Store/Actions/settingsActions';
import { fetchTagDetails, fetchTags } from 'Store/Actions/tagActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import Tag from './Tag';

function Tags() {
  const dispatch = useDispatch();
  const { items, isFetching, isPopulated, error, details } = useSelector(
    createSortedSectionSelector<TagModel, TagsAppState>(
      'tags',
      sortByProp('label')
    )
  );

  const {
    isFetching: isDetailsFetching,
    isPopulated: isDetailsPopulated,
    error: detailsError,
  } = details;

  useEffect(() => {
    dispatch(fetchTags());
    dispatch(fetchTagDetails());
    dispatch(fetchDelayProfiles());
    dispatch(fetchImportLists());
    dispatch(fetchNotifications());
    dispatch(fetchReleaseProfiles());
    dispatch(fetchIndexers());
    dispatch(fetchDownloadClients());
  }, [dispatch]);

  if (!items.length) {
    return (
      <Alert kind={kinds.INFO}>{translate('NoTagsHaveBeenAddedYet')}</Alert>
    );
  }

  return (
    <PageSectionContent
      errorMessage={translate('TagsLoadError')}
      error={error || detailsError}
      isFetching={isFetching || isDetailsFetching}
      isPopulated={isPopulated || isDetailsPopulated}
    >
      <div className={settingsCardStyles.grid}>
        {items.map((item) => {
          return <Tag key={item.id} {...item} />;
        })}
      </div>
    </PageSectionContent>
  );
}

export default Tags;
