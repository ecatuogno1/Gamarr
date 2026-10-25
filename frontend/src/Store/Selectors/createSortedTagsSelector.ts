import { createSelector } from 'reselect';
import createTagsSelector from './createTagsSelector';

function createSortedTagsSelector() {
  return createSelector(createTagsSelector(), (tags) => {
    return [...tags].sort((a, b) => a.label.localeCompare(b.label));
  });
}

export default createSortedTagsSelector;
