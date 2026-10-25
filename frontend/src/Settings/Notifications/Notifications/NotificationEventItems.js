import PropTypes from 'prop-types';
import React from 'react';
import CheckInput from 'Components/Form/CheckInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import translate from 'Utilities/String/translate';
import styles from './NotificationEventItems.css';

function NotificationEventItems(props) {
  const {
    item,
    onInputChange
  } = props;

  const {
    onGrab,
    onDownload,
    onUpgrade,
    onRename,
    onGameAdded,
    onGameDelete,
    onGameFileDelete,
    onGameFileDeleteForUpgrade,
    onHealthIssue,
    onHealthRestored,
    onApplicationUpdate,
    onManualInteractionRequired,
    supportsOnGrab,
    supportsOnDownload,
    supportsOnUpgrade,
    supportsOnRename,
    supportsOnGameAdded,
    supportsOnGameDelete,
    supportsOnGameFileDelete,
    supportsOnGameFileDeleteForUpgrade,
    supportsOnApplicationUpdate,
    supportsOnManualInteractionRequired,
    supportsOnHealthIssue,
    supportsOnHealthRestored,
    includeHealthWarnings
  } = item;

  return (
    <FormRow>
      <FormLabel>{translate('NotificationTriggers')}</FormLabel>
      <div>
        <FormInputHelpText
          text={translate('NotificationTriggersHelpText')}
          link="https://github.com/ecatuogno1/Gamarr/wiki/settings#connections"
        />
        <div className={styles.events}>
          <div>
            <CheckInput
              name="onGrab"
              helpText={translate('OnGrab')}
              isDisabled={!supportsOnGrab.value}
              {...onGrab}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onDownload"
              helpText={translate('OnFileImport')}
              isDisabled={!supportsOnDownload.value}
              {...onDownload}
              onChange={onInputChange}
            />
          </div>

          {
            onDownload.value &&
              <div>
                <CheckInput
                  name="onUpgrade"
                  helpText={translate('OnFileUpgrade')}
                  isDisabled={!supportsOnUpgrade.value}
                  {...onUpgrade}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <CheckInput
              name="onRename"
              helpText={translate('OnRename')}
              isDisabled={!supportsOnRename.value}
              {...onRename}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onGameAdded"
              helpText={translate('OnGameAdded')}
              isDisabled={!supportsOnGameAdded.value}
              {...onGameAdded}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onGameDelete"
              helpText={translate('OnGameDelete')}
              isDisabled={!supportsOnGameDelete.value}
              {...onGameDelete}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onGameFileDelete"
              helpText={translate('OnGameFileDelete')}
              isDisabled={!supportsOnGameFileDelete.value}
              {...onGameFileDelete}
              onChange={onInputChange}
            />
          </div>

          {
            onGameFileDelete.value &&
              <div>
                <CheckInput
                  name="onGameFileDeleteForUpgrade"
                  helpText={translate('OnGameFileDeleteForUpgrade')}
                  isDisabled={!supportsOnGameFileDeleteForUpgrade.value}
                  {...onGameFileDeleteForUpgrade}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <CheckInput
              name="onHealthIssue"
              helpText={translate('OnHealthIssue')}
              isDisabled={!supportsOnHealthIssue.value}
              {...onHealthIssue}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onHealthRestored"
              helpText={translate('OnHealthRestored')}
              isDisabled={!supportsOnHealthRestored.value}
              {...onHealthRestored}
              onChange={onInputChange}
            />
          </div>

          {
            (onHealthIssue.value || onHealthRestored.value) &&
              <div>
                <CheckInput
                  name="includeHealthWarnings"
                  helpText={translate('IncludeHealthWarnings')}
                  isDisabled={!supportsOnHealthIssue.value}
                  {...includeHealthWarnings}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <CheckInput
              name="onApplicationUpdate"
              helpText={translate('OnApplicationUpdate')}
              isDisabled={!supportsOnApplicationUpdate.value}
              {...onApplicationUpdate}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onManualInteractionRequired"
              helpText={translate('OnManualInteractionRequired')}
              isDisabled={!supportsOnManualInteractionRequired.value}
              {...onManualInteractionRequired}
              onChange={onInputChange}
            />
          </div>
        </div>
      </div>
    </FormRow>
  );
}

NotificationEventItems.propTypes = {
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default NotificationEventItems;
