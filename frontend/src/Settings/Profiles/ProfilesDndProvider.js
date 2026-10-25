import PropTypes from 'prop-types';
import React from 'react';
import { DndProvider } from 'react-dnd-multi-backend';
import HTML5toTouch from 'react-dnd-multi-backend/dist/esm/HTML5toTouch';

// react-dnd-multi-backend ships no type declarations, so the provider lives in
// a JS module. Only a single DragDrop context can exist, so it wraps the whole
// profiles page to allow editing quality profiles and reordering delay profiles.

function ProfilesDndProvider({ children }) {
  return (
    <DndProvider options={HTML5toTouch}>
      {children}
    </DndProvider>
  );
}

ProfilesDndProvider.propTypes = {
  children: PropTypes.node
};

export default ProfilesDndProvider;
