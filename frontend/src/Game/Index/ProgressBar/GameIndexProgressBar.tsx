import React from 'react';
import { useSelector } from 'react-redux';
import ProgressBar from 'Components/ProgressBar';
import { GameStatus } from 'Game/Game';
import createGameQueueItemsDetailsSelector, {
  GameQueueDetails,
} from 'Game/Index/createGameQueueDetailsSelector';
import { GameFile } from 'GameFile/GameFile';
import { sizes } from 'Helpers/Props';
import getProgressBarKind from 'Utilities/Game/getProgressBarKind';
import translate from 'Utilities/String/translate';
import styles from './GameIndexProgressBar.css';

interface GameIndexProgressBarProps {
  gameId: number;
  gameFile?: GameFile;
  monitored: boolean;
  status: GameStatus;
  hasFile: boolean;
  isAvailable: boolean;
  width: number;
  detailedProgressBar: boolean;
  bottomRadius?: boolean;
  isStandAlone?: boolean;
}

function GameIndexProgressBar({
  gameId,
  gameFile,
  monitored,
  status,
  hasFile,
  isAvailable,
  width,
  detailedProgressBar,
  bottomRadius,
  isStandAlone,
}: GameIndexProgressBarProps) {
  const queueDetails: GameQueueDetails = useSelector(
    createGameQueueItemsDetailsSelector(gameId)
  );

  const progress = 100;
  const queueStatusText =
    queueDetails.count > 0 ? translate('Downloading') : null;

  let gameStatus = translate('NotAvailable');
  if (hasFile) {
    gameStatus = gameFile?.quality?.quality.name ?? translate('Downloaded');
  } else if (status === 'deleted') {
    gameStatus = translate('Deleted');
  } else if (isAvailable && !hasFile) {
    gameStatus = translate('Missing');
  }

  let containerClassName: string | undefined = undefined;

  if (!isStandAlone) {
    if (detailedProgressBar) {
      containerClassName = styles.progressDetailed;
    } else {
      containerClassName = bottomRadius
        ? styles.progressRadius
        : styles.progress;
    }
  }

  return (
    <ProgressBar
      className={
        detailedProgressBar ? styles.progressBarDetailed : styles.progressBar
      }
      containerClassName={containerClassName}
      progress={progress}
      kind={getProgressBarKind(
        status,
        monitored,
        hasFile,
        isAvailable,
        queueDetails.count > 0
      )}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      width={width}
      text={queueStatusText ? queueStatusText : gameStatus}
    />
  );
}

export default GameIndexProgressBar;
