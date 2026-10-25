import { createStore } from 'redux';
import createReducers, { defaultState } from 'Store/Actions/createReducers';
import middlewares from 'Store/Middleware/middlewares';

let store = null;

export function getAppStore() {
  return store;
}

function createAppStore(history) {
  store = createStore(
    createReducers(history),
    defaultState,
    middlewares(history)
  );

  return store;
}

export default createAppStore;
