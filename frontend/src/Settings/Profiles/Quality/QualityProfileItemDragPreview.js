import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { DragLayer } from 'react-dnd';
import DragPreviewLayer from 'Components/DragPreviewLayer';
import { QUALITY_PROFILE_ITEM } from 'Helpers/dragTypes';
import QualityProfileItem from './QualityProfileItem';
import styles from './QualityProfileItemDragPreview.css';

// Must match QualityProfileItemDragPreview.css (.dragPreview width) and the
// drag handle width + right padding in QualityProfileItem.css.
const PREVIEW_WIDTH = 380;
const DRAG_HANDLE_WIDTH = 28;
const ITEM_PADDING = 12;

function collectDragLayer(monitor) {
  return {
    item: monitor.getItem(),
    itemType: monitor.getItemType(),
    currentOffset: monitor.getSourceClientOffset()
  };
}

class QualityProfileItemDragPreview extends Component {

  //
  // Render

  render() {
    const {
      item,
      itemType,
      currentOffset
    } = this.props;

    if (!currentOffset || itemType !== QUALITY_PROFILE_ITEM) {
      return null;
    }

    // The offset is shifted because the drag handle is on the right edge of the
    // list item and the preview is wider than the drag handle.

    const { x, y } = currentOffset;
    const handleOffset = PREVIEW_WIDTH - DRAG_HANDLE_WIDTH - ITEM_PADDING;
    const transform = `translate3d(${x - handleOffset}px, ${y}px, 0)`;

    const style = {
      position: 'absolute',
      WebkitTransform: transform,
      msTransform: transform,
      transform
    };

    const {
      editGroups,
      groupId,
      qualityId,
      name,
      allowed
    } = item;

    // TODO: Show a different preview for groups

    return (
      <DragPreviewLayer>
        <div
          className={styles.dragPreview}
          style={style}
        >
          <QualityProfileItem
            editGroups={editGroups}
            isPreview={true}
            qualityId={groupId || qualityId}
            name={name}
            allowed={allowed}
            isDragging={false}
          />
        </div>
      </DragPreviewLayer>
    );
  }
}

QualityProfileItemDragPreview.propTypes = {
  item: PropTypes.object,
  itemType: PropTypes.string,
  currentOffset: PropTypes.shape({
    x: PropTypes.number.isRequired,
    y: PropTypes.number.isRequired
  })
};

export default DragLayer(collectDragLayer)(QualityProfileItemDragPreview);
