import createAjaxRequest from 'Utilities/createAjaxRequest';

// Adds a title on several platforms: one POST /game per platform, in order, stopping at the first error.
// onAdded is called with every added game.
export default function addGamePlatforms(newGame, platformIds, onAdded) {
  return platformIds.reduce((promise, platformId) => {
    return promise.then(() => new Promise((resolve, reject) => {
      const { request } = createAjaxRequest({
        url: '/game',
        method: 'POST',
        dataType: 'json',
        contentType: 'application/json',
        data: JSON.stringify({ ...newGame, id: 0, platformId })
      });

      request.done((data) => {
        onAdded(data);
        resolve();
      });

      request.fail((xhr) => {
        reject(xhr);
      });
    }));
  }, Promise.resolve());
}
