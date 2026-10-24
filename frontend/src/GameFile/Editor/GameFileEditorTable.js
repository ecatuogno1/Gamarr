import PropTypes from 'prop-types';
import React from 'react';
import GameFileEditorTableContentConnector from './GameFileEditorTableContentConnector';
import styles from './GameFileEditorTable.css';

function GameFileEditorTable(props) {
  const {
    gameIds
  } = props;

  return (
    <div className={styles.container}>
      <GameFileEditorTableContentConnector
        gameIds={gameIds}
      />
    </div>
  );
}

GameFileEditorTable.propTypes = {
  gameIds: PropTypes.arrayOf(PropTypes.number).isRequired
};

export default GameFileEditorTable;
