import React from 'react';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import { GamePlatform } from 'Game/Game';
import GameSearchCell from 'Game/GameSearchCell';
import GameStatus from 'Game/GameStatus';
import GameTitleLink from 'Game/GameTitleLink';
import GameFileLanguages from 'GameFile/GameFileLanguages';
import { SelectStateInputProps } from 'typings/props';
import styles from './CutoffUnmetRow.css';

interface CutoffUnmetRowProps {
  id: number;
  gameFileId?: number;
  releaseDate?: string;
  lastSearchTime?: string;
  title: string;
  year: number;
  titleSlug: string;
  platform?: GamePlatform;
  isSelected?: boolean;
  columns: Column[];
  onSelectedChange: (options: SelectStateInputProps) => void;
}

function CutoffUnmetRow({
  id,
  gameFileId,
  releaseDate,
  lastSearchTime,
  title,
  year,
  titleSlug,
  platform,
  isSelected,
  columns,
  onSelectedChange,
}: CutoffUnmetRowProps) {
  if (!gameFileId) {
    return null;
  }

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'gameMetadata.sortTitle') {
          return (
            <TableRowCell key={name}>
              <GameTitleLink
                titleSlug={titleSlug}
                title={title}
                platform={platform}
              />
            </TableRowCell>
          );
        }

        if (name === 'gameMetadata.year') {
          return <TableRowCell key={name}>{year}</TableRowCell>;
        }

        if (name === 'gameMetadata.releaseDate') {
          return (
            <RelativeDateCell
              key={name}
              date={releaseDate}
              timeForToday={false}
            />
          );
        }

        if (name === 'languages') {
          return (
            <TableRowCell key={name} className={styles.languages}>
              <GameFileLanguages gameFileId={gameFileId} />
            </TableRowCell>
          );
        }

        if (name === 'games.lastSearchTime') {
          return (
            <RelativeDateCell
              key={name}
              date={lastSearchTime}
              includeSeconds={true}
            />
          );
        }

        if (name === 'status') {
          return (
            <TableRowCell key={name} className={styles.status}>
              <GameStatus
                gameId={id}
                gameFileId={gameFileId}
                gameEntity="wanted.cutoffUnmet"
              />
            </TableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <GameSearchCell
              key={name}
              gameId={id}
              gameEntity="wanted.cutoffUnmet"
            />
          );
        }

        return null;
      })}
    </TableRow>
  );
}

export default CutoffUnmetRow;
