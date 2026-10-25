// Sonarr v5 reads system status through react-query; Gamarr keeps it in redux.
// Same API, redux-backed.
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import SystemStatus from 'typings/SystemStatus';

export const useSystemStatusData = (): SystemStatus => {
  return useSelector((state: AppState) => state.system.status.item);
};

const useSystemStatus = () => {
  const { isFetching, isPopulated, error, item } = useSelector(
    (state: AppState) => state.system.status
  );

  return {
    isFetching,
    isFetched: isPopulated,
    isLoading: isFetching && !isPopulated,
    error,
    data: item,
  };
};

export default useSystemStatus;

export const useIsWindows = () => {
  const { isWindows } = useSystemStatusData();

  return isWindows;
};

export const useIsWindowsService = () => {
  const { isWindows, mode } = useSystemStatusData();

  return isWindows && mode === 'service';
};
