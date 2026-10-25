import React from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import ModelBase from 'App/ModelBase';
import AppState from 'App/State/AppState';
import Label from 'Components/Label';
import Button from 'Components/Link/Button';
import MiddleTruncate from 'Components/MiddleTruncate';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import GameTitleLink from 'Game/GameTitleLink';
import { kinds } from 'Helpers/Props';
import createAllGamesSelector from 'Store/Selectors/createAllGamesSelector';
import translate from 'Utilities/String/translate';
import TagDetailsDelayProfile from './TagDetailsDelayProfile';
import styles from './TagDetailsModalContent.css';

function findMatchingItems<T extends ModelBase>(ids: number[], items: T[]) {
  return items.filter((s) => {
    return ids.includes(s.id);
  });
}

function createUnorderedMatchingGamesSelector(gameIds: number[]) {
  return createSelector(createAllGamesSelector(), (games) =>
    findMatchingItems(gameIds, games)
  );
}

function createMatchingGamesSelector(gameIds: number[]) {
  return createSelector(
    createUnorderedMatchingGamesSelector(gameIds),
    (games) => {
      return games.sort((gameA, gameB) => {
        const sortTitleA = gameA.sortTitle;
        const sortTitleB = gameB.sortTitle;

        if (sortTitleA > sortTitleB) {
          return 1;
        } else if (sortTitleA < sortTitleB) {
          return -1;
        }

        return 0;
      });
    }
  );
}

function createMatchingItemSelector<T extends ModelBase>(
  ids: number[],
  selector: (state: AppState) => T[]
) {
  return createSelector(selector, (items) => findMatchingItems<T>(ids, items));
}

export interface TagDetailsModalContentProps {
  label: string;
  isTagUsed: boolean;
  delayProfileIds: number[];
  importListIds: number[];
  notificationIds: number[];
  releaseProfileIds: number[];
  indexerIds: number[];
  downloadClientIds: number[];
  autoTagIds: number[];
  gameIds: number[];
  onModalClose: () => void;
  onDeleteTagPress: () => void;
}

function TagDetailsModalContent({
  label,
  isTagUsed,
  delayProfileIds = [],
  importListIds = [],
  notificationIds = [],
  releaseProfileIds = [],
  indexerIds = [],
  downloadClientIds = [],
  autoTagIds = [],
  gameIds = [],
  onModalClose,
  onDeleteTagPress,
}: TagDetailsModalContentProps) {
  const games = useSelector(createMatchingGamesSelector(gameIds));

  const delayProfiles = useSelector(
    createMatchingItemSelector(
      delayProfileIds,
      (state: AppState) => state.settings.delayProfiles.items
    )
  );

  const importLists = useSelector(
    createMatchingItemSelector(
      importListIds,
      (state: AppState) => state.settings.importLists.items
    )
  );

  const notifications = useSelector(
    createMatchingItemSelector(
      notificationIds,
      (state: AppState) => state.settings.notifications.items
    )
  );

  const releaseProfiles = useSelector(
    createMatchingItemSelector(
      releaseProfileIds,
      (state: AppState) => state.settings.releaseProfiles.items
    )
  );

  const indexers = useSelector(
    createMatchingItemSelector(
      indexerIds,
      (state: AppState) => state.settings.indexers.items
    )
  );

  const downloadClients = useSelector(
    createMatchingItemSelector(
      downloadClientIds,
      (state: AppState) => state.settings.downloadClients.items
    )
  );

  const autoTags = useSelector(
    createMatchingItemSelector(
      autoTagIds,
      (state: AppState) => state.settings.autoTaggings.items
    )
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('TagDetails', { label })}</ModalHeader>

      <ModalBody>
        {isTagUsed ? null : (
          <div>{translate('TagIsNotUsedAndCanBeDeleted')}</div>
        )}

        {games.length ? (
          <ModalSection title={translate('Games')}>
            {games.map((item) => {
              return (
                <div key={item.id}>
                  <GameTitleLink
                    titleSlug={item.titleSlug}
                    title={item.title}
                  />
                </div>
              );
            })}
          </ModalSection>
        ) : null}

        {delayProfiles.length ? (
          <ModalSection title={translate('DelayProfile')}>
            {delayProfiles.map((item) => {
              const {
                id,
                preferredProtocol,
                enableUsenet,
                enableTorrent,
                usenetDelay,
                torrentDelay,
              } = item;

              return (
                <TagDetailsDelayProfile
                  key={id}
                  preferredProtocol={preferredProtocol}
                  enableUsenet={enableUsenet}
                  enableTorrent={enableTorrent}
                  usenetDelay={usenetDelay}
                  torrentDelay={torrentDelay}
                />
              );
            })}
          </ModalSection>
        ) : null}

        {notifications.length ? (
          <ModalSection title={translate('Connections')}>
            {notifications.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </ModalSection>
        ) : null}

        {importLists.length ? (
          <ModalSection title={translate('ImportLists')}>
            {importLists.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </ModalSection>
        ) : null}

        {releaseProfiles.length ? (
          <ModalSection title={translate('ReleaseProfiles')}>
            {releaseProfiles.map((item) => {
              return (
                <div key={item.id} className={styles.restriction}>
                  <div>
                    {item.required.map((r) => {
                      return (
                        <Label
                          key={r}
                          className={settingsCardStyles.truncatedLabel}
                          kind={kinds.SUCCESS}
                        >
                          <MiddleTruncate text={r} />
                        </Label>
                      );
                    })}
                  </div>

                  <div>
                    {item.ignored.map((i) => {
                      return (
                        <Label
                          key={i}
                          className={settingsCardStyles.truncatedLabel}
                          kind={kinds.DANGER}
                        >
                          <MiddleTruncate text={i} />
                        </Label>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </ModalSection>
        ) : null}

        {indexers.length ? (
          <ModalSection title={translate('Indexers')}>
            {indexers.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </ModalSection>
        ) : null}

        {downloadClients.length ? (
          <ModalSection title={translate('DownloadClients')}>
            {downloadClients.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </ModalSection>
        ) : null}

        {autoTags.length ? (
          <ModalSection title={translate('AutoTagging')}>
            {autoTags.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </ModalSection>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button
          className={styles.deleteButton}
          kind={kinds.DANGER}
          title={
            isTagUsed ? translate('TagCannotBeDeletedWhileInUse') : undefined
          }
          isDisabled={isTagUsed}
          onPress={onDeleteTagPress}
        >
          {translate('Delete')}
        </Button>

        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default TagDetailsModalContent;
