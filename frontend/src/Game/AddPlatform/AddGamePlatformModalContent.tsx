import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import CheckInput from 'Components/Form/CheckInput';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { GameAvailability, GameMonitor } from 'Game/Game';
import PlatformCheckList, {
  getPlatformIdsFromIgdbIds,
  getPlatformOptions,
} from 'Game/Title/PlatformCheckList';
import useGame from 'Game/useGame';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import { addGamePlatforms } from 'Store/Actions/gameActions';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './AddGamePlatformModalContent.css';

export interface AddGamePlatformModalContentProps {
  // A game of the title, the new platform versions start with its settings
  gameId: number;
  onModalClose: () => void;
}

function AddGamePlatformModalContent({
  gameId,
  onModalClose,
}: AddGamePlatformModalContentProps) {
  const dispatch = useDispatch();
  const game = useGame(gameId)!;
  const allGames = useSelector((state: AppState) => state.games.items);
  const platforms = useSelector(
    (state: AppState) => state.settings.platforms.items
  );
  const { isAddingPlatforms, addPlatformsError } = useSelector(
    (state: AppState) => state.games
  );
  // The add game default (addGame isn't typed in AppState)
  const searchForGameDefault = useSelector(
    (state: AppState) =>
      (
        state as unknown as {
          addGame: { defaults: { searchForGame: boolean } };
        }
      ).addGame.defaults.searchForGame
  );
  const wasAdding = usePrevious(isAddingPlatforms);

  const options = useMemo(() => {
    const ownedPlatformIds = allGames
      .filter((g) => g.igdbId === game.igdbId)
      .map((g) => g.platformId);

    return getPlatformOptions(
      platforms,
      getPlatformIdsFromIgdbIds(platforms, game.availablePlatformIgdbIds),
      ownedPlatformIds
    );
  }, [allGames, platforms, game]);

  const [platformIds, setPlatformIds] = useState<number[]>([]);
  const [rootFolderPath, setRootFolderPath] = useState(game.rootFolderPath);
  const [qualityProfileId, setQualityProfileId] = useState(
    game.qualityProfileId
  );
  const [monitor, setMonitor] = useState<GameMonitor>('gameOnly');
  const [minimumAvailability, setMinimumAvailability] =
    useState<GameAvailability>(game.minimumAvailability);
  const [tags, setTags] = useState<number[]>(game.tags);
  const [searchForGame, setSearchForGame] = useState(searchForGameDefault);

  const handleInputChange = useCallback(({ name, value }: InputChanged) => {
    switch (name) {
      case 'rootFolderPath':
        setRootFolderPath(value as string);
        break;
      case 'qualityProfileId':
        setQualityProfileId(parseInt(value as string));
        break;
      case 'monitor':
        setMonitor(value as GameMonitor);
        break;
      case 'minimumAvailability':
        setMinimumAvailability(value as GameAvailability);
        break;
      case 'tags':
        setTags(value as number[]);
        break;
      case 'searchForGame':
        setSearchForGame(value as boolean);
        break;
      default:
        break;
    }
  }, []);

  const handleAddPress = useCallback(() => {
    dispatch(
      addGamePlatforms({
        gameId,
        platformIds,
        rootFolderPath,
        qualityProfileId,
        monitor,
        minimumAvailability,
        tags,
        searchForGame,
      })
    );
  }, [
    gameId,
    platformIds,
    rootFolderPath,
    qualityProfileId,
    monitor,
    minimumAvailability,
    tags,
    searchForGame,
    dispatch,
  ]);

  useEffect(() => {
    if (wasAdding && !isAddingPlatforms && !addPlatformsError) {
      onModalClose();
    }
  }, [wasAdding, isAddingPlatforms, addPlatformsError, onModalClose]);

  const hasAddablePlatforms = options.some((option) => !option.isOwned);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('AddPlatformModalHeader', { title: game.title })}
      </ModalHeader>

      <ModalBody>
        <div className={styles.message}>
          {translate('AddPlatformModalMessage')}
        </div>

        <Form>
          <FormGroup>
            <FormLabel>{translate('Platforms')}</FormLabel>

            {hasAddablePlatforms ? (
              <PlatformCheckList
                options={options}
                selectedIds={platformIds}
                onChange={setPlatformIds}
              />
            ) : (
              <div className={styles.noPlatforms}>
                {translate('NoOtherPlatformsAvailable')}
              </div>
            )}
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('RootFolder')}</FormLabel>

            <FormInputGroup
              type={inputTypes.ROOT_FOLDER_SELECT}
              name="rootFolderPath"
              value={rootFolderPath}
              helpText={translate('AddPlatformRootFolderHelpText')}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Monitor')}</FormLabel>

            <FormInputGroup
              type={inputTypes.MONITOR_GAMES_SELECT}
              name="monitor"
              value={monitor}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('MinimumAvailability')}</FormLabel>

            <FormInputGroup
              type={inputTypes.AVAILABILITY_SELECT}
              name="minimumAvailability"
              value={minimumAvailability}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('QualityProfile')}</FormLabel>

            <FormInputGroup
              type={inputTypes.QUALITY_PROFILE_SELECT}
              name="qualityProfileId"
              value={qualityProfileId}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              value={tags}
              onChange={handleInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <label className={styles.searchLabelContainer}>
          <span className={styles.searchLabel}>
            {translate('StartSearchForMissingGame')}
          </span>

          <CheckInput
            containerClassName={styles.searchInputContainer}
            name="searchForGame"
            value={searchForGame}
            onChange={handleInputChange}
          />
        </label>

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          kind={kinds.SUCCESS}
          error={addPlatformsError}
          isSpinning={isAddingPlatforms}
          isDisabled={!platformIds.length}
          onPress={handleAddPress}
        >
          {platformIds.length > 1
            ? translate('AddCountPlatforms', { count: platformIds.length })
            : translate('AddPlatform')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddGamePlatformModalContent;
