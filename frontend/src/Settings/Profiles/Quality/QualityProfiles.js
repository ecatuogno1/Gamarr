import PropTypes from 'prop-types';
import React, { useCallback, useState } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import translate from 'Utilities/String/translate';
import EditQualityProfileModalConnector from './EditQualityProfileModalConnector';
import QualityProfile from './QualityProfile';

function QualityProfiles(props) {
  const {
    items,
    isDeleting,
    onConfirmDeleteQualityProfile,
    onCloneQualityProfilePress,
    ...otherProps
  } = props;

  const [isQualityProfileModalOpen, setIsQualityProfileModalOpen] = useState(false);

  const handleCloneQualityProfilePress = useCallback((id) => {
    onCloneQualityProfilePress(id);
    setIsQualityProfileModalOpen(true);
  }, [onCloneQualityProfilePress]);

  const handleAddQualityProfilePress = useCallback(() => {
    setIsQualityProfileModalOpen(true);
  }, []);

  const handleModalClose = useCallback(() => {
    setIsQualityProfileModalOpen(false);
  }, []);

  return (
    <PageSectionContent
      errorMessage={translate('QualityProfilesLoadError')}
      {...otherProps}
    >
      <div className={settingsCardStyles.grid}>
        {
          items.map((item) => {
            return (
              <QualityProfile
                key={item.id}
                {...item}
                isDeleting={isDeleting}
                onConfirmDeleteQualityProfile={onConfirmDeleteQualityProfile}
                onCloneQualityProfilePress={handleCloneQualityProfilePress}
              />
            );
          })
        }

        <AddCard
          label={translate('AddQualityProfile')}
          onPress={handleAddQualityProfilePress}
        />
      </div>

      <EditQualityProfileModalConnector
        isOpen={isQualityProfileModalOpen}
        onModalClose={handleModalClose}
      />
    </PageSectionContent>
  );
}

QualityProfiles.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isDeleting: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteQualityProfile: PropTypes.func.isRequired,
  onCloneQualityProfilePress: PropTypes.func.isRequired
};

export default QualityProfiles;
