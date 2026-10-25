import React from 'react';
import ProgressBar from 'Components/ProgressBar';
import GameIndexProgressBar from 'Game/Index/ProgressBar/GameIndexProgressBar';
import styles from 'Game/Index/ProgressBar/GameIndexProgressBar.css';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import { GameTitleSummary } from './gameTitle';

interface GameTitleProgressBarProps {
  summary: GameTitleSummary;
  width: number;
  detailedProgressBar: boolean;
  bottomRadius?: boolean;
  isStandAlone?: boolean;
}

export function getPlatformsPresentText(summary: GameTitleSummary) {
  return translate('PlatformsFilledOfTotal', {
    filled: summary.downloadedCount,
    total: summary.totalCount,
  });
}

// One platform: the game's own progress bar. Several: how many platform versions are downloaded.
function GameTitleProgressBar({
  summary,
  width,
  detailedProgressBar,
  bottomRadius,
  isStandAlone,
}: GameTitleProgressBarProps) {
  if (summary.totalCount === 1) {
    const { game } = summary.versions[0];

    return (
      <GameIndexProgressBar
        gameId={game.id}
        gameFile={game.gameFile}
        monitored={game.monitored}
        hasFile={game.hasFile}
        isAvailable={game.isAvailable}
        status={game.status}
        width={width}
        detailedProgressBar={detailedProgressBar}
        bottomRadius={bottomRadius}
        isStandAlone={isStandAlone}
      />
    );
  }

  let attachedClassName = bottomRadius
    ? styles.progressRadius
    : styles.progress;

  if (detailedProgressBar) {
    attachedClassName = styles.progressDetailed;
  }

  const text = getPlatformsPresentText(summary);

  return (
    <ProgressBar
      className={
        detailedProgressBar ? styles.progressBarDetailed : styles.progressBar
      }
      containerClassName={isStandAlone ? undefined : attachedClassName}
      progress={100}
      kind={summary.kind as 'primary'}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      width={width}
      title={summary.versions.map((v) => v.tooltip).join('\n')}
      text={text}
    />
  );
}

export default GameTitleProgressBar;
