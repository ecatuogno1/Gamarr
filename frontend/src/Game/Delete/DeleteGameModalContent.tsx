import React, { useCallback, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { Statistics } from 'Game/Game';
import { getTitleWithPlatform } from 'Game/Title/gameTitle';
import { useGameTitleVersions } from 'Game/Title/useGameTitle';
import useGame from 'Game/useGame';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import {
  bulkDeleteGame,
  deleteGame,
  setDeleteOption,
} from 'Store/Actions/gameActions';
import { CheckInputChanged } from 'typings/inputs';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './DeleteGameModalContent.css';

export interface DeleteGameModalContentProps {
  gameId: number;
  // Every platform version of the game's title, to offer removing all of them
  titleGameIds?: number[];
  onModalClose: () => void;
}

type DeleteScope = 'platform' | 'title';

function DeleteGameModalContent({
  gameId,
  titleGameIds = [],
  onModalClose,
}: DeleteGameModalContentProps) {
  const dispatch = useDispatch();
  const {
    title,
    path: platformPath,
    platform,
    collection,
    statistics: platformStatistics = {} as Statistics,
  } = useGame(gameId)!;
  const titleVersions = useGameTitleVersions(titleGameIds);
  const isMultiPlatform = titleVersions.length > 1;
  const [scope, setScope] = useState<DeleteScope>('platform');
  const isTitleScope = isMultiPlatform && scope === 'title';

  const path = isTitleScope
    ? titleVersions.map((game) => game.path).join(', ')
    : platformPath;

  const statistics = isTitleScope
    ? titleVersions.reduce(
        (acc, game) => ({
          ...acc,
          gameFileCount:
            acc.gameFileCount + (game.statistics?.gameFileCount ?? 0),
          sizeOnDisk: acc.sizeOnDisk + (game.statistics?.sizeOnDisk ?? 0),
        }),
        { ...platformStatistics, gameFileCount: 0, sizeOnDisk: 0 }
      )
    : platformStatistics;

  const scopeOptions = useMemo(
    () => [
      {
        key: 'platform',
        value: translate('DeletePlatformVersion', {
          platform: platform?.name ?? '',
        }),
      },
      {
        key: 'title',
        value: translate('DeleteAllPlatformVersions', {
          count: titleVersions.length,
        }),
      },
    ],
    [platform, titleVersions]
  );

  const handleScopeChange = useCallback(({ value }: { value: DeleteScope }) => {
    setScope(value);
  }, []);
  const { addImportExclusion } = useSelector(
    (state: AppState) => state.games.deleteOptions
  );

  const { gameFileCount = 0, sizeOnDisk = 0 } = statistics;

  const [deleteFiles, setDeleteFiles] = useState(false);

  const handleDeleteFilesChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setDeleteFiles(value);
    },
    []
  );

  const handleDeleteGameConfirmed = useCallback(() => {
    if (isTitleScope) {
      dispatch(
        bulkDeleteGame({
          gameIds: titleVersions.map((game) => game.id),
          deleteFiles,
          addImportExclusion,
        })
      );

      onModalClose();

      return;
    }

    dispatch(
      deleteGame({
        id: gameId,
        collectionIgdbId: collection?.igdbId,
        deleteFiles,
        addImportExclusion,
      })
    );

    onModalClose();
  }, [
    gameId,
    isTitleScope,
    titleVersions,
    collection,
    addImportExclusion,
    deleteFiles,
    dispatch,
    onModalClose,
  ]);

  const handleDeleteOptionChange = useCallback(
    ({ name, value }: CheckInputChanged) => {
      dispatch(setDeleteOption({ [name]: value }));
    },
    [dispatch]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('DeleteHeader', {
          title: getTitleWithPlatform(title, isTitleScope ? null : platform),
        })}
      </ModalHeader>

      <ModalBody>
        {isMultiPlatform ? (
          <FormGroup>
            <FormLabel>{translate('Remove')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="scope"
              value={scope}
              values={scopeOptions}
              helpText={translate('DeleteGamePlatformsHelpText')}
              onChange={handleScopeChange}
            />
          </FormGroup>
        ) : null}

        <div className={styles.pathContainer}>
          <Icon className={styles.pathIcon} name={icons.FOLDER} />

          {path}
        </div>

        <FormGroup>
          <FormLabel>{translate('AddListExclusion')}</FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="addImportExclusion"
            value={addImportExclusion}
            helpText={translate('AddListExclusionGameHelpText')}
            kind={kinds.DANGER}
            onChange={handleDeleteOptionChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>
            {gameFileCount === 0
              ? translate('DeleteGameFolder')
              : translate('DeleteGameFiles', { gameFileCount })}
          </FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="deleteFiles"
            value={deleteFiles}
            helpText={
              gameFileCount === 0
                ? translate('DeleteGameFolderHelpText')
                : translate('DeleteGameFilesHelpText')
            }
            kind={kinds.DANGER}
            onChange={handleDeleteFilesChange}
          />
        </FormGroup>

        {deleteFiles ? (
          <div className={styles.deleteFilesMessage}>
            <div>
              <InlineMarkdown
                data={translate('DeleteGameFolderConfirmation', { path })}
                blockClassName={styles.folderPath}
              />
            </div>

            {gameFileCount ? (
              <div className={styles.deleteCount}>
                {translate('DeleteGameFolderGameCount', {
                  gameFileCount,
                  size: formatBytes(sizeOnDisk),
                })}
              </div>
            ) : null}
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>

        <Button kind={kinds.DANGER} onPress={handleDeleteGameConfirmed}>
          {translate('Delete')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default DeleteGameModalContent;
