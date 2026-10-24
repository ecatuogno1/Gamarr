import React, { useMemo } from 'react';
import { shallowEqual, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import GamePlatformBadges from './GamePlatformBadges';
import { sortVersions } from './gameTitle';
import GameTitleProgressBar from './GameTitleProgressBar';
import useGameTitle from './useGameTitle';

function createTitleGameIdsSelector(igdbId: number) {
  return createSelector(
    (state: AppState) => state.games.items,
    (games) =>
      sortVersions(games.filter((game) => game.igdbId === igdbId)).map(
        (game) => game.id
      )
  );
}

interface TitleBadgesProps {
  className?: string;
  gameIds: number[];
}

function TitleBadges({ className, gameIds }: TitleBadgesProps) {
  const summary = useGameTitle(gameIds);

  return summary ? (
    <GamePlatformBadges className={className} versions={summary.versions} />
  ) : null;
}

interface LibraryPlatformBadgesProps {
  className?: string;
  igdbId: number;
}

// The platforms a title (e.g. a collection or discover entry) is in the library on, nothing when it isn't
function LibraryPlatformBadges({
  className,
  igdbId,
}: LibraryPlatformBadgesProps) {
  const selector = useMemo(() => createTitleGameIdsSelector(igdbId), [igdbId]);
  const gameIds = useSelector(selector, shallowEqual);

  return gameIds.length ? (
    <TitleBadges className={className} gameIds={gameIds} />
  ) : null;
}

interface TitleProgressBarProps {
  gameIds: number[];
  width: number;
  detailedProgressBar: boolean;
  bottomRadius?: boolean;
}

function TitleProgressBar({ gameIds, ...otherProps }: TitleProgressBarProps) {
  const summary = useGameTitle(gameIds);

  return summary ? (
    <GameTitleProgressBar summary={summary} {...otherProps} />
  ) : null;
}

interface LibraryTitleProgressBarProps
  extends Omit<TitleProgressBarProps, 'gameIds'> {
  igdbId: number;
}

// The progress of all platform versions of a title in the library
export function LibraryTitleProgressBar({
  igdbId,
  ...otherProps
}: LibraryTitleProgressBarProps) {
  const selector = useMemo(() => createTitleGameIdsSelector(igdbId), [igdbId]);
  const gameIds = useSelector(selector, shallowEqual);

  return gameIds.length ? (
    <TitleProgressBar gameIds={gameIds} {...otherProps} />
  ) : null;
}

export default LibraryPlatformBadges;
