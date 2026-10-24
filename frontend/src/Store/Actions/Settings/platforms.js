import { batchActions } from 'redux-batched-actions';
import { set, updateItem } from 'Store/Actions/baseActions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import { createThunk } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';

//
// Variables

const section = 'settings.platforms';

//
// Actions Types

export const FETCH_PLATFORMS = 'settings.platforms/fetchPlatforms';
export const SAVE_PLATFORM_ENABLED = 'settings.platforms/savePlatformEnabled';

//
// Action Creators

export const fetchPlatforms = createThunk(FETCH_PLATFORMS);
export const savePlatformEnabled = createThunk(SAVE_PLATFORM_ENABLED);

//
// Details

export default {

  //
  // State

  defaultState: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isSaving: false,
    saveError: null,
    items: []
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_PLATFORMS]: createFetchHandler(section, '/platform'),

    // Only Enabled can be changed, the rest of a platform is seeded reference data
    [SAVE_PLATFORM_ENABLED]: function(getState, payload, dispatch) {
      const { id, enabled } = payload;
      const platform = getState().settings.platforms.items.find((p) => p.id === id);

      if (!platform) {
        return;
      }

      dispatch(batchActions([
        updateItem({ section, id, enabled, isSaving: true }),
        set({ section, isSaving: true })
      ]));

      const promise = createAjaxRequest({
        url: `/platform/${id}`,
        method: 'PUT',
        data: JSON.stringify({ ...platform, enabled }),
        dataType: 'json'
      }).request;

      promise.done((data) => {
        dispatch(batchActions([
          updateItem({ section, ...data, isSaving: false }),
          set({ section, isSaving: false, saveError: null })
        ]));
      });

      promise.fail((xhr) => {
        dispatch(batchActions([
          updateItem({ section, id, enabled: platform.enabled, isSaving: false }),
          set({ section, isSaving: false, saveError: xhr })
        ]));
      });
    }
  },

  //
  // Reducers

  reducers: {

  }

};
