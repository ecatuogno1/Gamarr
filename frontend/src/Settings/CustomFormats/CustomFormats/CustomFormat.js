import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Label from 'Components/Label';
import MiddleTruncate from 'Components/MiddleTruncate';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditCustomFormatModalConnector from './EditCustomFormatModalConnector';
import ExportCustomFormatModal from './ExportCustomFormatModal';

class CustomFormat extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditCustomFormatModalOpen: false,
      isExportCustomFormatModalOpen: false,
      isDeleteCustomFormatModalOpen: false
    };
  }

  //
  // Listeners

  onEditCustomFormatPress = () => {
    this.setState({ isEditCustomFormatModalOpen: true });
  };

  onEditCustomFormatModalClose = () => {
    this.setState({ isEditCustomFormatModalOpen: false });
  };

  onExportCustomFormatPress = () => {
    this.setState({ isExportCustomFormatModalOpen: true });
  };

  onExportCustomFormatModalClose = () => {
    this.setState({ isExportCustomFormatModalOpen: false });
  };

  onDeleteCustomFormatPress = () => {
    this.setState({
      isEditCustomFormatModalOpen: false,
      isDeleteCustomFormatModalOpen: true
    });
  };

  onDeleteCustomFormatModalClose = () => {
    this.setState({ isDeleteCustomFormatModalOpen: false });
  };

  onConfirmDeleteCustomFormat = () => {
    this.props.onConfirmDeleteCustomFormat(this.props.id);
  };

  onCloneCustomFormatPress = () => {
    const {
      id,
      onCloneCustomFormatPress
    } = this.props;

    onCloneCustomFormatPress(id);
  };

  //
  // Render

  render() {
    const {
      id,
      name,
      specifications,
      isDeleting
    } = this.props;

    const conditionCount = specifications.length;
    const isActive = conditionCount > 0;

    return (
      <SettingsCard
        name={name}
        aria-label={translate('EditCustomFormatName', { name })}
        actions={
          <>
            <SettingsCardAction
              title={translate('CloneCustomFormat')}
              aria-label={translate('CloneCustomFormat')}
              name={icons.CLONE}
              onPress={this.onCloneCustomFormatPress}
            />

            <SettingsCardAction
              title={translate('ExportCustomFormat')}
              aria-label={translate('ExportCustomFormat')}
              name={icons.EXPORT}
              onPress={this.onExportCustomFormatPress}
            />
          </>
        }
        onPress={this.onEditCustomFormatPress}
      >
        <SettingsCardStatus
          dot={isActive ? 'active' : 'muted'}
          segments={[translate('ConditionsCount', { count: conditionCount })]}
        />

        {
          conditionCount ?
            <div className={settingsCardStyles.labels}>
              {
                specifications.map((item, index) => {
                  if (!item) {
                    return null;
                  }

                  let kind = kinds.DEFAULT;

                  if (item.required) {
                    kind = kinds.SUCCESS;
                  }

                  if (item.negate) {
                    kind = kinds.DANGER;
                  }

                  return (
                    <Label
                      key={index}
                      className={settingsCardStyles.truncatedLabel}
                      kind={kind}
                      dot={item.required || item.negate}
                    >
                      <MiddleTruncate text={item.name} />
                    </Label>
                  );
                })
              }
            </div> :
            null
        }

        <EditCustomFormatModalConnector
          id={id}
          isOpen={this.state.isEditCustomFormatModalOpen}
          onModalClose={this.onEditCustomFormatModalClose}
          onDeleteCustomFormatPress={this.onDeleteCustomFormatPress}
        />

        <ExportCustomFormatModal
          id={id}
          isOpen={this.state.isExportCustomFormatModalOpen}
          onModalClose={this.onExportCustomFormatModalClose}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteCustomFormatModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteCustomFormat')}
          message={translate('DeleteCustomFormatMessageText', { name })}
          confirmLabel={translate('Delete')}
          isSpinning={isDeleting}
          onConfirm={this.onConfirmDeleteCustomFormat}
          onCancel={this.onDeleteCustomFormatModalClose}
        />
      </SettingsCard>
    );
  }
}

CustomFormat.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  specifications: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDeleting: PropTypes.bool.isRequired,
  onConfirmDeleteCustomFormat: PropTypes.func.isRequired,
  onCloneCustomFormatPress: PropTypes.func.isRequired
};

export default CustomFormat;
