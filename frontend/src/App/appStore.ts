// Sonarr v5 exposes app state through a zustand store with this API. Gamarr
// still keeps app state in redux, so this module offers the same API backed by
// the redux store, letting v5 components be used unchanged.
import { shallowEqual, useSelector } from 'react-redux';
import AppState, { AppSectionState } from 'App/State/AppState';
import * as appActions from 'Store/Actions/appActions';
import { getAppStore } from 'Store/createAppStore';

type AppValues = AppSectionState;
type Dimensions = AppSectionState['dimensions'];

function getState(): AppValues {
  return (getAppStore().getState() as AppState).app;
}

function dispatch(action: unknown) {
  getAppStore().dispatch(action);
}

export const useAppValues = <K extends keyof AppValues>(...keys: K[]) => {
  return useSelector((state: AppState) => {
    return keys.reduce((acc, key) => {
      acc[key] = state.app[key];
      return acc;
    }, {} as Pick<AppValues, K>);
  }, shallowEqual);
};

export const useAppValue = <K extends keyof AppValues>(key: K) => {
  return useSelector((state: AppState) => state.app[key]);
};

export const useAppDimensions = () => {
  return useSelector((state: AppState) => state.app.dimensions, shallowEqual);
};

export const useAppDimension = <K extends keyof Dimensions>(key: K) => {
  return useSelector((state: AppState) => state.app.dimensions[key]);
};

export const getAppDimensions = () => {
  return getState().dimensions;
};

export const getAppValues = <K extends keyof AppValues>(...keys: K[]) => {
  const state = getState();

  return keys.reduce((acc, key) => {
    acc[key] = state[key];
    return acc;
  }, {} as Pick<AppValues, K>);
};

export const getAppValue = <K extends keyof AppValues>(key: K) => {
  return getState()[key];
};

export const saveDimensions = (payload: { width: number; height: number }) => {
  dispatch(appActions.saveDimensions(payload));
};

export const setVersion = (payload: { version: string }) => {
  dispatch(appActions.setVersion(payload));
};

export const setIsSidebarVisible = (payload: { isSidebarVisible: boolean }) => {
  dispatch(appActions.setIsSidebarVisible(payload));
};

export const toggleIsSidebarVisible = () => {
  setIsSidebarVisible({ isSidebarVisible: !getState().isSidebarVisible });
};

export const setAppValue = (payload: Partial<AppValues>) => {
  dispatch(appActions.setAppValue(payload));
};

export const pingServer = () => {
  dispatch(appActions.pingServer());
};
