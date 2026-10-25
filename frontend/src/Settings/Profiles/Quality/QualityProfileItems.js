import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import Measure from 'Components/Measure';
import translate from 'Utilities/String/translate';
import QualityProfileItemDragPreview from './QualityProfileItemDragPreview';
import QualityProfileItemDragSource from './QualityProfileItemDragSource';
import styles from './QualityProfileItems.css';

class QualityProfileItems extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      qualitiesHeight: 0,
      qualitiesHeightEditGroups: 0
    };
  }

  //
  // Listeners

  onMeasure = ({ height }) => {
    if (this.props.editGroups) {
      this.setState({
        qualitiesHeightEditGroups: height
      });
    } else {
      this.setState({ qualitiesHeight: height });
    }
  };

  onToggleEditGroupsMode = () => {
    this.props.onToggleEditGroupsMode();
  };

  //
  // Render

  render() {
    const {
      editGroups,
      dropQualityIndex,
      dropPosition,
      qualityProfileItems,
      errors,
      warnings,
      ...otherProps
    } = this.props;

    const {
      qualitiesHeight,
      qualitiesHeightEditGroups
    } = this.state;

    const isDragging = dropQualityIndex !== null;
    const isDraggingUp = isDragging && dropPosition === 'above';
    const isDraggingDown = isDragging && dropPosition === 'below';
    const minHeight = editGroups ? qualitiesHeightEditGroups : qualitiesHeight;

    return (
      <div className={styles.qualitiesSection}>
        <div className={styles.headingRow}>
          <div className={styles.headingCluster}>
            <h3 className={styles.heading}>{translate('Qualities')}</h3>
          </div>

          <div className={styles.modeCluster} role="group">
            <button
              type="button"
              className={editGroups ? styles.modeCell : styles.modeCellActive}
              aria-pressed={!editGroups}
              onClick={editGroups ? this.onToggleEditGroupsMode : undefined}
            >
              {translate('Qualities')}
            </button>

            <button
              type="button"
              className={editGroups ? styles.modeCellActive : styles.modeCell}
              aria-pressed={editGroups}
              onClick={editGroups ? undefined : this.onToggleEditGroupsMode}
            >
              {translate('Groups')}
            </button>
          </div>
        </div>

        <FormInputHelpText text={translate('QualitiesHelpText')} />

        {
          errors.map((error, index) => {
            return (
              <FormInputHelpText
                key={index}
                text={error.message}
                isError={true}
                isCheckInput={false}
              />
            );
          })
        }

        {
          warnings.map((warning, index) => {
            return (
              <FormInputHelpText
                key={index}
                text={warning.message}
                isWarning={true}
                isCheckInput={false}
              />
            );
          })
        }

        <Measure
          whitelist={['height']}
          includeMargin={false}
          onMeasure={this.onMeasure}
        >
          <div
            className={styles.qualities}
            style={{ minHeight: `${minHeight}px` }}
          >
            {
              qualityProfileItems.map(({ id, name, allowed, quality, items }, index) => {
                const identifier = quality ? quality.id : id;

                return (
                  <QualityProfileItemDragSource
                    key={identifier}
                    editGroups={editGroups}
                    groupId={id}
                    qualityId={quality && quality.id}
                    name={quality ? quality.name : name}
                    allowed={allowed}
                    items={items}
                    qualityIndex={`${index + 1}`}
                    isInGroup={false}
                    isDragging={isDragging}
                    isDraggingUp={isDraggingUp}
                    isDraggingDown={isDraggingDown}
                    {...otherProps}
                  />
                );
              }).reverse()
            }

            <QualityProfileItemDragPreview />
          </div>
        </Measure>
      </div>
    );
  }
}

QualityProfileItems.propTypes = {
  editGroups: PropTypes.bool.isRequired,
  dragQualityIndex: PropTypes.string,
  dropQualityIndex: PropTypes.string,
  dropPosition: PropTypes.string,
  qualityProfileItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  errors: PropTypes.arrayOf(PropTypes.object),
  warnings: PropTypes.arrayOf(PropTypes.object),
  onToggleEditGroupsMode: PropTypes.func.isRequired
};

QualityProfileItems.defaultProps = {
  errors: [],
  warnings: []
};

export default QualityProfileItems;
