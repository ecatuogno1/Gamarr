import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { icons, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AddDownloadClientPresetMenuItem from './AddDownloadClientPresetMenuItem';
import styles from './AddDownloadClientItem.css';

class AddDownloadClientItem extends Component {

  //
  // Listeners

  onDownloadClientSelect = () => {
    const {
      implementation
    } = this.props;

    this.props.onDownloadClientSelect({ implementation });
  };

  //
  // Render

  render() {
    const {
      implementation,
      implementationName,
      infoLink,
      presets,
      onDownloadClientSelect
    } = this.props;

    const hasPresets = !!presets && !!presets.length;

    return (
      <div
        className={styles.downloadClient}
      >
        <Link
          className={styles.underlay}
          aria-label={translate('AddDownloadClientImplementation', { implementationName })}
          onPress={this.onDownloadClientSelect}
        />

        <div className={styles.overlay}>
          <div className={styles.name}>
            {implementationName}
          </div>

          <div className={styles.actions}>
            {
              hasPresets ?
                <Menu
                  className={styles.presetsMenu}
                  alignMenu="right"
                >
                  <Button
                    className={styles.presetsMenuButton}
                    size={sizes.SMALL}
                  >
                    {translate('Presets')}
                  </Button>

                  <MenuContent>
                    {
                      presets.map((preset) => {
                        return (
                          <AddDownloadClientPresetMenuItem
                            key={preset.name}
                            name={preset.name}
                            implementation={implementation}
                            onPress={onDownloadClientSelect}
                          />
                        );
                      })
                    }
                  </MenuContent>
                </Menu> :
                null
            }

            <Link
              className={styles.infoLink}
              to={infoLink}
              title={translate('MoreInfo')}
              aria-label={translate('MoreInfo')}
            >
              <Icon
                name={icons.INFO}
                size={18}
              />
            </Link>
          </div>
        </div>
      </div>
    );
  }
}

AddDownloadClientItem.propTypes = {
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  infoLink: PropTypes.string.isRequired,
  presets: PropTypes.arrayOf(PropTypes.object),
  onDownloadClientSelect: PropTypes.func.isRequired
};

export default AddDownloadClientItem;
