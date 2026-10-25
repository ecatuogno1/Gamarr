// Sonarr v5 reads UI settings through react-query; Gamarr keeps them in redux.
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import UiSettings from 'typings/Settings/UiSettings';

export const useUiSettingsValues = (): UiSettings => {
  return useSelector((state: AppState) => state.settings.ui.item);
};

function useUiSettings() {
  return useSelector((state: AppState) => state.settings.ui);
}

export default useUiSettings;
