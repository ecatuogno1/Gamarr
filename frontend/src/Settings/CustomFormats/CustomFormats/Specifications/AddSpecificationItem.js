import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { icons, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AddSpecificationPresetMenuItem from './AddSpecificationPresetMenuItem';
import styles from './AddSpecificationItem.css';

class AddSpecificationItem extends Component {

  //
  // Listeners

  onSpecificationSelect = () => {
    const {
      implementation
    } = this.props;

    this.props.onSpecificationSelect({ implementation });
  };

  //
  // Render

  render() {
    const {
      implementation,
      implementationName,
      infoLink,
      presets,
      onSpecificationSelect
    } = this.props;

    const hasPresets = !!presets && !!presets.length;

    return (
      <div
        className={styles.specification}
      >
        <Link
          className={styles.underlay}
          aria-label={translate('AddConditionImplementation', { implementationName })}
          onPress={this.onSpecificationSelect}
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
                      presets.map((preset, index) => {
                        return (
                          <AddSpecificationPresetMenuItem
                            key={index}
                            name={preset.name}
                            implementation={implementation}
                            onPress={onSpecificationSelect}
                          />
                        );
                      })
                    }
                  </MenuContent>
                </Menu> :
                null
            }

            {
              infoLink ?
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
                </Link> :
                null
            }
          </div>
        </div>
      </div>
    );
  }
}

AddSpecificationItem.propTypes = {
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  infoLink: PropTypes.string.isRequired,
  presets: PropTypes.arrayOf(PropTypes.object),
  onSpecificationSelect: PropTypes.func.isRequired
};

export default AddSpecificationItem;
