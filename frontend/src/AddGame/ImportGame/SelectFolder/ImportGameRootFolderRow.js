import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import { icons, kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './ImportGameRootFolderRow.css';

function ImportGameRootFolderRow(props) {
  const {
    id,
    path,
    accessible,
    freeSpace,
    unmappedFolders,
    onDeletePress
  } = props;

  const isUnavailable = accessible === false;
  const importableCount = unmappedFolders.length;
  const hasFreeSpace = freeSpace != null && !isNaN(freeSpace);

  return (
    <div
      className={classNames(
        styles.row,
        isUnavailable && styles.unavailable
      )}
    >
      {
        isUnavailable ?
          null :
          <Link
            className={styles.underlay}
            to={`/add/import/${id}`}
            aria-label={translate('ImportFromRootFolder', { path })}
          />
      }

      <div className={styles.content}>
        <Icon
          className={styles.folderIcon}
          name={icons.ROOT_FOLDER}
          size={20}
        />

        <div className={styles.info}>
          <span className={styles.path}>{path}</span>

          <div className={styles.meta}>
            {
              isUnavailable ?
                <span>{translate('RootFolderNotAccessible')}</span> :
                <>
                  <span className={styles.importable}>
                    {
                      importableCount > 0 ?
                        translate('RootFolderFoldersToImport', {
                          count: importableCount
                        }) :
                        translate('RootFolderNothingToImport')
                    }
                  </span>

                  {
                    hasFreeSpace ?
                      <>
                        <span className={styles.metaSep}>·</span>

                        <span className={styles.free}>
                          {translate('RootFolderSelectFreeSpace', {
                            freeSpace: formatBytes(freeSpace)
                          })}
                        </span>
                      </> :
                      null
                  }
                </>
            }
          </div>
        </div>

        {
          isUnavailable ?
            <Label kind={kinds.DANGER} dot={false}>
              {translate('Unavailable')}
            </Label> :
            null
        }

        <IconButton
          title={translate('RemoveRootFolder')}
          aria-label={translate('RemoveRootFolder')}
          name={icons.REMOVE}
          onPress={onDeletePress}
        />
      </div>
    </div>
  );
}

ImportGameRootFolderRow.propTypes = {
  id: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  accessible: PropTypes.bool,
  freeSpace: PropTypes.number,
  unmappedFolders: PropTypes.arrayOf(PropTypes.object).isRequired,
  onDeletePress: PropTypes.func.isRequired
};

ImportGameRootFolderRow.defaultProps = {
  unmappedFolders: []
};

export default ImportGameRootFolderRow;
