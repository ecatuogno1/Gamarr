import PropTypes from 'prop-types';
import React, { Component } from 'react';
import GameMinimumAvailabilityPopoverContent from 'AddGame/GameMinimumAvailabilityPopoverContent';
import CheckInput from 'Components/Form/CheckInput';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import GamePoster from 'Game/GamePoster';
import PlatformCheckList, { getPlatformGameFolder } from 'Game/Title/PlatformCheckList';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './AddNewGameModalContent.css';

class AddNewGameModalContent extends Component {

  //
  // Listeners

  onQualityProfileIdChange = ({ value }) => {
    this.props.onInputChange({ name: 'qualityProfileId', value: parseInt(value) });
  };

  onAddGamePress = () => {
    this.props.onAddGamePress();
  };

  //
  // Render

  render() {
    const {
      title,
      year,
      overview,
      images,
      selectedPlatformIds,
      platformOptions,
      isAdding,
      addError,
      validationErrors,
      validationWarnings,
      rootFolderPath,
      monitor,
      qualityProfileId,
      minimumAvailability,
      searchForGame,
      folder,
      tags,
      isSmallScreen,
      isWindows,
      onModalClose,
      onInputChange,
      onPlatformsChange
    } = this.props;

    // Each platform version gets its own platform folder
    const platformFolders = selectedPlatformIds.map((id) => getPlatformGameFolder(folder, platformOptions, id));
    const gameFolder = platformFolders.length ? platformFolders[0] : folder;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {title}

          {
            !title.contains(year) && !!year &&
              <span className={styles.year}>({year})</span>
          }
        </ModalHeader>

        <ModalBody>
          <div className={styles.container}>
            {
              !isSmallScreen &&
                <GamePoster
                  className={styles.poster}
                  images={images}
                  size={250}
                  lazy={false}
                />
            }

            <div className={styles.info}>
              {overview ? (
                <p className={styles.intro}>{overview}</p>
              ) : null}

              <Form
                validationErrors={validationErrors}
                validationWarnings={validationWarnings}
              >
                <FormRow>
                  <FormLabel>{translate('Platforms')}</FormLabel>

                  <FormInputHelpText
                    text={translate('AddNewGamePlatformsHelpText')}
                  />

                  <PlatformCheckList
                    options={platformOptions}
                    selectedIds={selectedPlatformIds}
                    onChange={onPlatformsChange}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>{translate('RootFolder')}</FormLabel>

                  <FormInputHelpText
                    text={platformFolders.length > 1 ?
                      translate('AddNewGameRootFolderPlatformsHelpText', {
                        folders: platformFolders.map((f) => `'${f}'`).join(', ')
                      }) :
                      translate('AddNewGameRootFolderHelpText', { folder: gameFolder })}
                  />

                  <FormInput
                    type={inputTypes.ROOT_FOLDER_SELECT}
                    name="rootFolderPath"
                    valueOptions={{
                      gameFolder,
                      isWindows
                    }}
                    selectedValueOptions={{
                      gameFolder,
                      isWindows
                    }}
                    onChange={onInputChange}
                    {...rootFolderPath}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>
                    {translate('Monitor')}
                  </FormLabel>

                  <FormInput
                    type={inputTypes.MONITOR_GAMES_SELECT}
                    name="monitor"
                    onChange={onInputChange}
                    {...monitor}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>
                    {translate('MinimumAvailability')}

                    <Popover
                      anchor={
                        <Icon
                          className={styles.labelIcon}
                          name={icons.INFO}
                        />
                      }
                      title={translate('MinimumAvailability')}
                      body={<GameMinimumAvailabilityPopoverContent />}
                      position={tooltipPositions.RIGHT}
                    />
                  </FormLabel>

                  <FormInputHelpText
                    text={translate('MoreInfo')}
                    link="https://github.com/ecatuogno1/Gamarr/wiki/faq#what-is-minimum-availability"
                  />

                  <FormInput
                    type={inputTypes.AVAILABILITY_SELECT}
                    name="minimumAvailability"
                    onChange={onInputChange}
                    {...minimumAvailability}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>{translate('QualityProfile')}</FormLabel>

                  <FormInput
                    type={inputTypes.QUALITY_PROFILE_SELECT}
                    name="qualityProfileId"
                    onChange={this.onQualityProfileIdChange}
                    {...qualityProfileId}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>{translate('Tags')}</FormLabel>

                  <FormInput
                    type={inputTypes.TAG}
                    name="tags"
                    onChange={onInputChange}
                    {...tags}
                  />
                </FormRow>
              </Form>
            </div>
          </div>
        </ModalBody>

        <ModalFooter className={styles.modalFooter}>
          <div className={styles.searchOptions}>
            <span className={styles.searchLabel}>{translate('Search')}</span>

            <CheckInput
              containerClassName={styles.searchOption}
              name="searchForGame"
              helpText={translate('StartSearchForMissingGame')}
              onChange={onInputChange}
              {...searchForGame}
            />
          </div>

          <div className={styles.actions}>
            <Button onPress={onModalClose}>{translate('Cancel')}</Button>

            <SpinnerErrorButton
              className={styles.addButton}
              kind={kinds.PRIMARY}
              isSpinning={isAdding}
              isDisabled={!selectedPlatformIds.length}
              error={addError}
              onPress={this.onAddGamePress}
            >
              {
                selectedPlatformIds.length > 1 ?
                  translate('AddGameOnCountPlatforms', { count: selectedPlatformIds.length }) :
                  translate('AddGame')
              }
            </SpinnerErrorButton>
          </div>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddNewGameModalContent.propTypes = {
  selectedPlatformIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  platformOptions: PropTypes.arrayOf(PropTypes.object).isRequired,
  onPlatformsChange: PropTypes.func.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  validationErrors: PropTypes.arrayOf(PropTypes.object),
  validationWarnings: PropTypes.arrayOf(PropTypes.object),
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  minimumAvailability: PropTypes.object.isRequired,
  searchForGame: PropTypes.object.isRequired,
  folder: PropTypes.string.isRequired,
  tags: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isWindows: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onAddGamePress: PropTypes.func.isRequired
};

export default AddNewGameModalContent;
