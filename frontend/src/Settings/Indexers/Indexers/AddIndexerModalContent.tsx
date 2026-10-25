import React, { useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import { fetchIndexerSchema } from 'Store/Actions/settingsActions';
import Indexer from 'typings/Indexer';
import translate from 'Utilities/String/translate';
import AddIndexerItem from './AddIndexerItem';
import styles from './AddIndexerModalContent.css';

export interface AddIndexerModalContentProps {
  onIndexerSelect: () => void;
  onModalClose: () => void;
}

function AddIndexerModalContent({
  onIndexerSelect,
  onModalClose,
}: AddIndexerModalContentProps) {
  const dispatch = useDispatch();

  const { isSchemaFetching, isSchemaPopulated, schemaError, schema } =
    useSelector((state: AppState) => state.settings.indexers);

  const { usenetIndexers, torrentIndexers } = useMemo(() => {
    return schema.reduce<{
      usenetIndexers: Indexer[];
      torrentIndexers: Indexer[];
    }>(
      (acc, item) => {
        if (item.protocol === 'usenet') {
          acc.usenetIndexers.push(item);
        } else if (item.protocol === 'torrent') {
          acc.torrentIndexers.push(item);
        }

        return acc;
      },
      {
        usenetIndexers: [],
        torrentIndexers: [],
      }
    );
  }, [schema]);

  useEffect(() => {
    dispatch(fetchIndexerSchema());
  }, [dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('AddIndexer')}</ModalHeader>

      <ModalBody>
        {isSchemaFetching ? <LoadingIndicator /> : null}

        {!isSchemaFetching && !!schemaError ? (
          <Alert kind={kinds.DANGER}>{translate('AddIndexerError')}</Alert>
        ) : null}

        {isSchemaPopulated && !schemaError ? (
          <div className={styles.indexers}>
            <div className={styles.group}>
              <h3 className={styles.groupHeading}>{translate('Usenet')}</h3>

              {usenetIndexers.map((indexer) => (
                <AddIndexerItem
                  key={indexer.implementation}
                  {...indexer}
                  implementation={indexer.implementation}
                  onIndexerSelect={onIndexerSelect}
                />
              ))}
            </div>

            <div className={styles.group}>
              <h3 className={styles.groupHeading}>{translate('Torrents')}</h3>

              {torrentIndexers.map((indexer) => (
                <AddIndexerItem
                  key={indexer.implementation}
                  {...indexer}
                  implementation={indexer.implementation}
                  onIndexerSelect={onIndexerSelect}
                />
              ))}
            </div>
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddIndexerModalContent;
