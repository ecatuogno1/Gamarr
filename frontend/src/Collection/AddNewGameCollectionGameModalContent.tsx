import React, { useCallback, useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import GameMinimumAvailabilityPopoverContent from 'AddGame/GameMinimumAvailabilityPopoverContent';
import AppState from 'App/State/AppState';
import useGameCollection from 'Collection/useGameCollection';
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
import { Image } from 'Game/Game';
import GamePoster from 'Game/GamePoster';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import {
  addGame,
  setGameCollectionValue,
} from 'Store/Actions/gameCollectionActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import useIsWindows from 'System/useIsWindows';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './AddNewGameCollectionGameModalContent.css';

export interface AddNewGameCollectionGameModalContentProps {
  igdbId: number;
  title: string;
  year: number;
  overview?: string;
  images: Image[];
  collectionId: number;
  folder: string;
  onModalClose: () => void;
}

function AddNewGameCollectionGameModalContent({
  igdbId,
  title,
  year,
  overview,
  images,
  collectionId,
  folder,
  onModalClose,
}: AddNewGameCollectionGameModalContentProps) {
  const dispatch = useDispatch();

  const collection = useGameCollection(collectionId)!;

  const { isSmallScreen } = useSelector(createDimensionsSelector());
  const isWindows = useIsWindows();

  const { isAdding, addError, pendingChanges } = useSelector(
    (state: AppState) => state.gameCollections
  );

  const wasAdding = usePrevious(isAdding);

  const { settings, validationErrors, validationWarnings } = useMemo(() => {
    const options = {
      rootFolderPath: collection.rootFolderPath,
      monitor: collection.monitored ? 'gameOnly' : 'none',
      qualityProfileId: collection.qualityProfileId,
      minimumAvailability: collection.minimumAvailability,
      searchForGame: collection.searchOnAdd,
      tags: collection.tags || [],
    };

    return selectSettings(options, pendingChanges, addError);
  }, [collection, pendingChanges, addError]);

  const {
    monitor,
    qualityProfileId,
    minimumAvailability,
    rootFolderPath,
    searchForGame,
    tags,
  } = settings;

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error actions aren't typed
      dispatch(setGameCollectionValue({ name, value }));
    },
    [dispatch]
  );

  const handleAddGamePress = useCallback(() => {
    dispatch(
      addGame({
        igdbId,
        title,
        rootFolderPath: rootFolderPath.value,
        monitor: monitor.value,
        qualityProfileId: qualityProfileId.value,
        minimumAvailability: minimumAvailability.value,
        searchForGame: searchForGame.value,
        tags: tags.value,
      })
    );
  }, [
    igdbId,
    title,
    rootFolderPath,
    monitor,
    qualityProfileId,
    minimumAvailability,
    searchForGame,
    tags,
    dispatch,
  ]);

  useEffect(() => {
    if (!isAdding && wasAdding && !addError) {
      onModalClose();
    }
  }, [isAdding, wasAdding, addError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {title}

        {!title.includes(String(year)) && year ? (
          <span className={styles.year}>({year})</span>
        ) : null}
      </ModalHeader>

      <ModalBody>
        <div className={styles.container}>
          {isSmallScreen ? null : (
            <GamePoster
              className={styles.poster}
              images={images}
              size={250}
              lazy={false}
            />
          )}

          <div className={styles.info}>
            {overview ? <p className={styles.intro}>{overview}</p> : null}

            <Form
              validationErrors={validationErrors}
              validationWarnings={validationWarnings}
            >
              <FormRow>
                <FormLabel>{translate('RootFolder')}</FormLabel>

                <FormInputHelpText
                  text={translate('AddNewGameRootFolderHelpText', {
                    folder,
                  })}
                />

                <FormInput
                  type={inputTypes.ROOT_FOLDER_SELECT}
                  name="rootFolderPath"
                  valueOptions={{
                    gameFolder: folder,
                    isWindows,
                  }}
                  selectedValueOptions={{
                    gameFolder: folder,
                    isWindows,
                  }}
                  {...rootFolderPath}
                  onChange={handleInputChange}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('Monitor')}</FormLabel>

                <FormInput
                  type={inputTypes.MONITOR_GAMES_SELECT}
                  name="monitor"
                  {...monitor}
                  onChange={handleInputChange}
                />
              </FormRow>

              <FormRow>
                <FormLabel>
                  {translate('MinimumAvailability')}

                  <Popover
                    anchor={
                      <Icon className={styles.labelIcon} name={icons.INFO} />
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
                  {...minimumAvailability}
                  onChange={handleInputChange}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('QualityProfile')}</FormLabel>

                <FormInput
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  {...qualityProfileId}
                  onChange={handleInputChange}
                />
              </FormRow>

              <FormRow>
                <FormLabel>{translate('Tags')}</FormLabel>

                <FormInput
                  type={inputTypes.TAG}
                  name="tags"
                  {...tags}
                  onChange={handleInputChange}
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
            {...searchForGame}
            onChange={handleInputChange}
          />
        </div>

        <div className={styles.actions}>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <SpinnerErrorButton
            className={styles.addButton}
            kind={kinds.PRIMARY}
            isSpinning={isAdding}
            error={addError}
            onPress={handleAddGamePress}
          >
            {translate('AddGame')}
          </SpinnerErrorButton>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddNewGameCollectionGameModalContent;
