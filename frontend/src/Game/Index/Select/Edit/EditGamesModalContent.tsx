import React, { useCallback, useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import MoveGameModal from 'Game/MoveGame/MoveGameModal';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import styles from './EditGamesModalContent.css';

interface SavePayload {
  monitored?: boolean;
  qualityProfileId?: number;
  platformId?: number;
  minimumAvailability?: string;
  rootFolderPath?: string;
  moveFiles?: boolean;
}

interface EditGamesModalContentProps {
  gameIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

const NO_CHANGE = 'noChange';

const monitoredOptions = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true,
  },
  {
    key: 'monitored',
    get value() {
      return translate('Monitored');
    },
  },
  {
    key: 'unmonitored',
    get value() {
      return translate('Unmonitored');
    },
  },
];

// The editor answers with a list of validation failures (one per conflicting game) or a single message
function getSaveErrorMessages(saveError: AppState['games']['saveError']) {
  if (!saveError) {
    return [];
  }

  const { responseJSON } = saveError;

  if (Array.isArray(responseJSON)) {
    return responseJSON.map((failure) => failure.errorMessage);
  }

  return [getErrorMessage(saveError, translate('EditGamesSaveFailed'))];
}

function EditGamesModalContent(props: EditGamesModalContentProps) {
  const { gameIds, onSavePress, onModalClose } = props;

  const { isSaving, saveError } = useSelector((state: AppState) => state.games);
  const wasSaving = usePrevious(isSaving);
  const [hasSubmitted, setHasSubmitted] = useState(false);

  const [monitored, setMonitored] = useState(NO_CHANGE);
  const [qualityProfileId, setQualityProfileId] = useState<string | number>(
    NO_CHANGE
  );
  const [platformId, setPlatformId] = useState<string | number>(NO_CHANGE);
  const [minimumAvailability, setMinimumAvailability] = useState(NO_CHANGE);
  const [rootFolderPath, setRootFolderPath] = useState(NO_CHANGE);
  const [isConfirmMoveModalOpen, setIsConfirmMoveModalOpen] = useState(false);

  const save = useCallback(
    (moveFiles: boolean) => {
      let hasChanges = false;
      const payload: SavePayload = {};

      if (monitored !== NO_CHANGE) {
        hasChanges = true;
        payload.monitored = monitored === 'monitored';
      }

      if (qualityProfileId !== NO_CHANGE) {
        hasChanges = true;
        payload.qualityProfileId = qualityProfileId as number;
      }

      if (platformId !== NO_CHANGE) {
        hasChanges = true;
        payload.platformId = platformId as number;
      }

      if (minimumAvailability !== NO_CHANGE) {
        hasChanges = true;
        payload.minimumAvailability = minimumAvailability as string;
      }

      if (rootFolderPath !== NO_CHANGE) {
        hasChanges = true;
        payload.rootFolderPath = rootFolderPath;
      }

      if (rootFolderPath !== NO_CHANGE || platformId !== NO_CHANGE) {
        payload.moveFiles = moveFiles;
      }

      if (hasChanges) {
        // Stays open until the save succeeds, so a rejected change (e.g. a platform conflict) is shown here
        setHasSubmitted(true);
        onSavePress(payload);
      } else {
        onModalClose();
      }
    },
    [
      monitored,
      qualityProfileId,
      platformId,
      minimumAvailability,
      rootFolderPath,
      onSavePress,
      onModalClose,
    ]
  );

  const onInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      switch (name) {
        case 'monitored':
          setMonitored(value as string);
          break;
        case 'qualityProfileId':
          setQualityProfileId(value as string);
          break;
        case 'platformId':
          setPlatformId(value as number);
          break;
        case 'minimumAvailability':
          setMinimumAvailability(value as string);
          break;
        case 'rootFolderPath':
          setRootFolderPath(value as string);
          break;
        default:
          console.warn('EditGamesModalContent Unknown Input');
      }
    },
    [setMonitored]
  );

  useEffect(() => {
    if (hasSubmitted && wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [hasSubmitted, wasSaving, isSaving, saveError, onModalClose]);

  const onSavePressWrapper = useCallback(() => {
    if (rootFolderPath === NO_CHANGE && platformId === NO_CHANGE) {
      save(false);
    } else {
      setIsConfirmMoveModalOpen(true);
    }
  }, [rootFolderPath, platformId, save]);

  const onCancelPress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
  }, [setIsConfirmMoveModalOpen]);

  const onDoNotMoveGamePress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
    save(false);
  }, [setIsConfirmMoveModalOpen, save]);

  const onMoveGamePress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
    save(true);
  }, [setIsConfirmMoveModalOpen, save]);

  const selectedCount = gameIds.length;
  const saveErrorMessages =
    hasSubmitted && !isSaving ? getSaveErrorMessages(saveError) : [];

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditSelectedGames')}</ModalHeader>

      <ModalBody>
        {saveErrorMessages.length ? (
          <Alert kind={kinds.DANGER}>
            <div>{translate('EditGamesSaveFailed')}</div>
            <ul>
              {saveErrorMessages.map((message, index) => (
                <li key={index}>{message}</li>
              ))}
            </ul>
          </Alert>
        ) : null}

        <FormGroup>
          <FormLabel>{translate('Monitored')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="monitored"
            value={monitored}
            values={monitoredOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('QualityProfile')}</FormLabel>

          <FormInputGroup
            type={inputTypes.QUALITY_PROFILE_SELECT}
            name="qualityProfileId"
            value={qualityProfileId}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('Platform')}</FormLabel>

          <FormInputGroup
            type={inputTypes.PLATFORM_SELECT}
            name="platformId"
            value={platformId}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            helpText={translate('EditGamesPlatformHelpText')}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('MinimumAvailability')}</FormLabel>

          <FormInputGroup
            type={inputTypes.AVAILABILITY_SELECT}
            name="minimumAvailability"
            value={minimumAvailability}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('RootFolder')}</FormLabel>

          <FormInputGroup
            type={inputTypes.ROOT_FOLDER_SELECT}
            name="rootFolderPath"
            value={rootFolderPath}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            selectedValueOptions={{ includeFreeSpace: false }}
            helpText={translate('GameEditRootFolderHelpText')}
            onChange={onInputChange}
          />
        </FormGroup>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div className={styles.selected}>
          {translate('GamesSelectedInterp', { count: selectedCount })}
        </div>

        <div>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <SpinnerButton
            isSpinning={hasSubmitted && isSaving}
            onPress={onSavePressWrapper}
          >
            {translate('ApplyChanges')}
          </SpinnerButton>
        </div>
      </ModalFooter>

      <MoveGameModal
        isOpen={isConfirmMoveModalOpen}
        destinationRootFolder={
          rootFolderPath === NO_CHANGE ? undefined : rootFolderPath
        }
        destinationPlatform={
          rootFolderPath === NO_CHANGE && platformId !== NO_CHANGE
        }
        onModalClose={onCancelPress}
        onSavePress={onDoNotMoveGamePress}
        onMoveGamePress={onMoveGamePress}
      />
    </ModalContent>
  );
}

export default EditGamesModalContent;
