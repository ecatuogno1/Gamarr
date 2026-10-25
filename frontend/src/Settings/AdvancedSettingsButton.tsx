import classNames from 'classnames';
import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import { toggleAdvancedSettings } from 'Store/Actions/settingsActions';
import translate from 'Utilities/String/translate';
import styles from './AdvancedSettingsButton.css';

interface AdvancedSettingsButtonProps {
  showLabel: boolean;
}

function AdvancedSettingsButton({ showLabel }: AdvancedSettingsButtonProps) {
  const showAdvancedSettings = useSelector(
    (state: AppState) => state.settings.advancedSettings
  );
  const dispatch = useDispatch();

  const handlePress = useCallback(() => {
    dispatch(toggleAdvancedSettings());
  }, [dispatch]);

  return (
    <Link
      className={styles.button}
      title={
        showAdvancedSettings
          ? translate('ShownClickToHide')
          : translate('HiddenClickToShow')
      }
      onPress={handlePress}
    >
      <div className={styles.iconWrapper}>
        <Icon name={icons.ADVANCED_SETTINGS} size={16} />

        <span
          className={classNames(
            styles.indicatorContainer,
            showAdvancedSettings ? styles.enabled : styles.disabled
          )}
        >
          <Icon
            name={showAdvancedSettings ? icons.CHECK_CIRCLE : icons.FATAL}
            size={9}
          />
        </span>
      </div>

      {showLabel ? (
        <span className={styles.label}>
          {showAdvancedSettings
            ? translate('HideAdvanced')
            : translate('ShowAdvanced')}
        </span>
      ) : null}
    </Link>
  );
}

export default AdvancedSettingsButton;
