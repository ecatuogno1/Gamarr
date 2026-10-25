import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import Measure from 'Components/Measure';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import DelayProfile from './DelayProfile';
import DelayProfileDragPreview from './DelayProfileDragPreview';
import DelayProfileDragSource from './DelayProfileDragSource';
import EditDelayProfileModalConnector from './EditDelayProfileModalConnector';
import styles from './DelayProfiles.css';

class DelayProfiles extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddDelayProfileModalOpen: false,
      width: 0
    };
  }

  //
  // Listeners

  onAddDelayProfilePress = () => {
    this.setState({ isAddDelayProfileModalOpen: true });
  };

  onModalClose = () => {
    this.setState({ isAddDelayProfileModalOpen: false });
  };

  onMeasure = ({ width }) => {
    this.setState({ width });
  };

  //
  // Render

  render() {
    const {
      defaultProfile,
      items,
      tagList,
      dragIndex,
      dropIndex,
      onConfirmDeleteDelayProfile,
      ...otherProps
    } = this.props;

    const {
      isAddDelayProfileModalOpen,
      width
    } = this.state;

    const isDragging = dropIndex !== null;
    const isDraggingUp = isDragging && dropIndex < dragIndex;
    const isDraggingDown = isDragging && dropIndex > dragIndex;

    return (
      <PageSectionContent
        errorMessage={translate('DelayProfilesLoadError')}
        {...otherProps}
      >
        <div className={styles.headerRow}>
          <div className={styles.colDrag} />

          <div className={styles.colScope}>{translate('Tags')}</div>

          <div className={styles.colProto}>{translate('PreferredProtocol')}</div>

          <div className={styles.colUsenet}>{translate('UsenetDelay')}</div>

          <div className={styles.colTorrent}>{translate('TorrentDelay')}</div>

          <div className={styles.colActions} />
        </div>

        <Measure onMeasure={this.onMeasure}>
          <div className={styles.delayList}>
            {
              items.map((item, index) => {
                return (
                  <DelayProfileDragSource
                    key={item.id}
                    tagList={tagList}
                    {...item}
                    {...otherProps}
                    index={index}
                    isDragging={isDragging}
                    isDraggingUp={isDraggingUp}
                    isDraggingDown={isDraggingDown}
                    onConfirmDeleteDelayProfile={onConfirmDeleteDelayProfile}
                  />
                );
              })
            }

            <DelayProfileDragPreview
              width={width}
            />

            {
              defaultProfile ?
                <DelayProfile
                  tagList={tagList}
                  isDragging={false}
                  onConfirmDeleteDelayProfile={onConfirmDeleteDelayProfile}
                  {...defaultProfile}
                /> :
                null
            }

            <Link
              className={styles.addRow}
              onPress={this.onAddDelayProfilePress}
            >
              <Icon name={icons.ADD} size={12} />
              {translate('AddDelayProfile')}
            </Link>
          </div>
        </Measure>

        <EditDelayProfileModalConnector
          isOpen={isAddDelayProfileModalOpen}
          onModalClose={this.onModalClose}
        />
      </PageSectionContent>
    );
  }
}

DelayProfiles.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  defaultProfile: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  dragIndex: PropTypes.number,
  dropIndex: PropTypes.number,
  onConfirmDeleteDelayProfile: PropTypes.func.isRequired
};

export default DelayProfiles;
