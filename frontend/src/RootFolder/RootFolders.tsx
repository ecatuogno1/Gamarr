import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { kinds } from 'Helpers/Props';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import createRootFoldersSelector from 'Store/Selectors/createRootFoldersSelector';
import translate from 'Utilities/String/translate';
import RootFolderRow from './RootFolderRow';
import styles from './RootFolders.css';

interface RootFoldersProps {
  isSelectable?: boolean;
}

function RootFolders({ isSelectable = false }: RootFoldersProps) {
  const { isFetching, isPopulated, error, items } = useSelector(
    createRootFoldersSelector()
  );

  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(fetchRootFolders());
  }, [dispatch]);

  if (isFetching && !isPopulated) {
    return <LoadingIndicator />;
  }

  if (!isFetching && !!error) {
    return (
      <Alert kind={kinds.DANGER}>{translate('RootFoldersLoadError')}</Alert>
    );
  }

  return (
    <div className={styles.list}>
      {items.map((rootFolder) => (
        <RootFolderRow
          key={rootFolder.id}
          id={rootFolder.id}
          path={rootFolder.path}
          accessible={rootFolder.accessible}
          freeSpace={rootFolder.freeSpace}
          unmappedFolders={rootFolder.unmappedFolders}
          isSelectable={isSelectable}
        />
      ))}
    </div>
  );
}

export default RootFolders;
