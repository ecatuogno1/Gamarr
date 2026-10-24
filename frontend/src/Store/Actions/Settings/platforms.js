import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.platforms';

//
// Actions Types

export const FETCH_PLATFORMS = 'settings.platforms/fetchPlatforms';

//
// Action Creators

export const fetchPlatforms = createThunk(FETCH_PLATFORMS);

//
// Details

export default {

  //
  // State

  defaultState: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_PLATFORMS]: createFetchHandler(section, '/platform')
  },

  //
  // Reducers

  reducers: {

  }

};
