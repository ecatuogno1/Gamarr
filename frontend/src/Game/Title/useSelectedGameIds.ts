import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import AppState from 'App/State/AppState';
import getSelectedIds from 'Utilities/Table/getSelectedIds';

// The library index selects titles (by IGDB id), the editor works on every platform version of them
function useSelectedGameIds() {
  const [selectState] = useSelect();
  const { selectedState } = selectState;
  const allGames = useSelector((state: AppState) => state.games.items);

  return useMemo(() => {
    const igdbIds = new Set(getSelectedIds(selectedState));

    return allGames
      .filter((game) => igdbIds.has(game.igdbId))
      .map((game) => game.id);
  }, [selectedState, allGames]);
}

export default useSelectedGameIds;
