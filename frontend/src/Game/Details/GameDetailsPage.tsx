import React, { useCallback, useEffect, useMemo } from 'react';
import { useSelector } from 'react-redux';
import { useHistory, useParams } from 'react-router';
import NotFound from 'Components/NotFound';
import Game from 'Game/Game';
import { getTitleLink, sortVersions } from 'Game/Title/gameTitle';
import usePrevious from 'Helpers/Hooks/usePrevious';
import createAllGamesSelector from 'Store/Selectors/createAllGamesSelector';
import getPathWithUrlBase from 'Utilities/getPathWithUrlBase';
import translate from 'Utilities/String/translate';
import GameDetails from './GameDetails';

// /game/{igdbId} opens a title, /game/{igdbId}-{platformId} (a game's titleSlug) opens it with that platform selected
export function parseTitleSlug(titleSlug: string) {
  const match = /^(\d+)(?:-(\d+))?$/.exec(titleSlug);

  if (!match) {
    return undefined;
  }

  return {
    igdbId: parseInt(match[1]),
    platformId: match[2] ? parseInt(match[2]) : undefined,
  };
}

function findVersions(allGames: Game[], titleSlug: string) {
  const parsed = parseTitleSlug(titleSlug);

  if (parsed) {
    return {
      igdbId: parsed.igdbId,
      platformId: parsed.platformId,
      versions: sortVersions(
        allGames.filter((game) => game.igdbId === parsed.igdbId)
      ),
    };
  }

  // IGDB slug ("super-mario-world")
  const game = allGames.find(
    (g) => g.igdbSlug === titleSlug || g.titleSlug === titleSlug
  );

  return {
    igdbId: game?.igdbId ?? 0,
    platformId: undefined,
    versions: game
      ? sortVersions(allGames.filter((g) => g.igdbId === game.igdbId))
      : [],
  };
}

function GameDetailsPage() {
  const allGames = useSelector(createAllGamesSelector());
  const { titleSlug } = useParams<{ titleSlug: string }>();
  const history = useHistory();

  const { igdbId, platformId, versions } = useMemo(
    () => findVersions(allGames, titleSlug),
    [allGames, titleSlug]
  );

  // The selected platform, or the first version with a file
  const selectedGame =
    versions.find((game) => game.platformId === platformId) ??
    versions.find((game) => game.hasFile && game.monitored) ??
    versions[0];

  const gameIds = useMemo(() => versions.map((game) => game.id), [versions]);
  const selectedGameId = selectedGame?.id;
  const previousSelectedGameId = usePrevious(selectedGameId);
  const previousCount = usePrevious(versions.length);

  useEffect(() => {
    if (!versions.length && previousCount) {
      history.push(`${window.Gamarr.urlBase}/`);
    }
  }, [versions, previousCount, history]);

  useEffect(() => {
    // The selected platform version was deleted, show the title's other platforms
    if (
      platformId &&
      previousSelectedGameId &&
      versions.length &&
      !versions.some((game) => game.platformId === platformId)
    ) {
      history.replace(getPathWithUrlBase(getTitleLink(igdbId)));
    }
  }, [igdbId, platformId, versions, previousSelectedGameId, history]);

  const onPlatformSelect = useCallback(
    (gameId: number) => {
      const game = versions.find((g) => g.id === gameId);

      if (game) {
        history.replace(
          getPathWithUrlBase(getTitleLink(game.igdbId, game.platformId))
        );
      }
    },
    [versions, history]
  );

  if (!selectedGame) {
    return <NotFound message={translate('GameCannotBeFound')} />;
  }

  return (
    <GameDetails
      gameId={selectedGame.id}
      titleGameIds={gameIds}
      onPlatformSelect={onPlatformSelect}
    />
  );
}

export default GameDetailsPage;
