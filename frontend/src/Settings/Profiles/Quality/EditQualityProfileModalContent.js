import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Measure from 'Components/Measure';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import translate from 'Utilities/String/translate';
import QualityProfileFormatItems from './QualityProfileFormatItems';
import QualityProfileItems from './QualityProfileItems';
import styles from './EditQualityProfileModalContent.css';

const MODAL_BODY_PADDING = dimensions.modalBodyPadding;

function getCustomFormatRender(formatItems, otherProps) {
  return (
    <QualityProfileFormatItems
      profileFormatItems={formatItems.value}
      errors={formatItems.errors}
      warnings={formatItems.warnings}
      {...otherProps}
    />
  );
}

class EditQualityProfileModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      headerHeight: 0,
      bodyHeight: 0,
      footerHeight: 0
    };
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      headerHeight,
      bodyHeight,
      footerHeight
    } = this.state;

    if (
      headerHeight > 0 &&
      bodyHeight > 0 &&
      footerHeight > 0 &&
      (
        headerHeight !== prevState.headerHeight ||
        bodyHeight !== prevState.bodyHeight ||
        footerHeight !== prevState.footerHeight
      )
    ) {
      const padding = MODAL_BODY_PADDING * 2;

      this.props.onContentHeightChange(
        headerHeight + bodyHeight + footerHeight + padding
      );
    }
  }

  //
  // Listeners

  onHeaderMeasure = ({ height }) => {
    if (height > this.state.headerHeight) {
      this.setState({ headerHeight: height });
    }
  };

  onBodyMeasure = ({ height }) => {

    if (height > this.state.bodyHeight) {
      this.setState({ bodyHeight: height });
    }
  };

  onFooterMeasure = ({ height }) => {
    if (height > this.state.footerHeight) {
      this.setState({ footerHeight: height });
    }
  };

  //
  // Render

  render() {
    const {
      editGroups,
      isFetching,
      error,
      isSaving,
      saveError,
      qualities,
      customFormats,
      languages,
      item,
      isInUse,
      onInputChange,
      onCutoffChange,
      onLanguageChange,
      onSavePress,
      onModalClose,
      onDeleteQualityProfilePress,
      ...otherProps
    } = this.props;

    const {
      id,
      name,
      upgradeAllowed,
      cutoff,
      minFormatScore,
      minUpgradeFormatScore,
      cutoffFormatScore,
      language,
      items,
      formatItems
    } = item;

    const languageId = language ? language.value.id : 0;

    return (
      <ModalContent onModalClose={onModalClose}>
        <Measure
          whitelist={['height']}
          includeMargin={false}
          onMeasure={this.onHeaderMeasure}
        >
          <ModalHeader subtitle={id ? name.previousValue ?? name.value : undefined}>
            {id ? translate('EditQualityProfile') : translate('AddQualityProfile')}
          </ModalHeader>
        </Measure>

        <ModalBody>
          <Measure
            whitelist={['height']}
            onMeasure={this.onBodyMeasure}
          >
            <div>
              {
                isFetching &&
                  <LoadingIndicator />
              }

              {
                !isFetching && !!error &&
                  <Alert kind={kinds.DANGER}>
                    {translate('AddQualityProfileError')}
                  </Alert>
              }

              {
                !isFetching && !error &&
                  <Form
                    {...otherProps}
                  >
                    <div className={styles.formGroupsContainer}>
                      <div className={styles.formGroupWrapper}>
                        <FormRow>
                          <FormLabel>
                            {translate('Name')}
                          </FormLabel>

                          <FormInput
                            type={inputTypes.TEXT}
                            name="name"
                            {...name}
                            onChange={onInputChange}
                          />
                        </FormRow>

                        <FormRow>
                          <FormLabel>
                            {translate('UpgradesAllowed')}
                          </FormLabel>

                          <FormInputHelpText text={translate('UpgradesAllowedHelpText')} />

                          <FormInput
                            type={inputTypes.CHECK}
                            name="upgradeAllowed"
                            {...upgradeAllowed}
                            onChange={onInputChange}
                          />
                        </FormRow>

                        {
                          upgradeAllowed.value &&
                            <FormRow>
                              <FormLabel>
                                {translate('UpgradeUntil')}
                              </FormLabel>

                              <FormInputHelpText text={translate('UpgradeUntilGameHelpText')} />

                              <FormInput
                                type={inputTypes.SELECT}
                                name="cutoff"
                                {...cutoff}
                                values={qualities}
                                onChange={onCutoffChange}
                              />
                            </FormRow>
                        }

                        {
                          formatItems.value.length > 0 &&
                            <FormRow>
                              <FormLabel>
                                {translate('MinimumCustomFormatScore')}
                              </FormLabel>

                              <FormInputHelpText text={translate('MinimumCustomFormatScoreHelpText')} />

                              <FormInput
                                type={inputTypes.NUMBER}
                                name="minFormatScore"
                                {...minFormatScore}
                                onChange={onInputChange}
                              />
                            </FormRow>
                        }

                        {
                          upgradeAllowed.value && formatItems.value.length > 0 &&
                            <FormRow>
                              <FormLabel>
                                {translate('UpgradeUntilCustomFormatScore')}
                              </FormLabel>

                              <FormInputHelpText text={translate('UpgradeUntilCustomFormatScoreGameHelpText')} />

                              <FormInput
                                type={inputTypes.NUMBER}
                                name="cutoffFormatScore"
                                {...cutoffFormatScore}
                                onChange={onInputChange}
                              />
                            </FormRow>
                        }

                        {
                          upgradeAllowed.value && formatItems.value.length > 0 ?
                            <FormRow>
                              <FormLabel>
                                {translate('MinimumCustomFormatScoreIncrement')}
                              </FormLabel>

                              <FormInputHelpText text={translate('MinimumCustomFormatScoreIncrementHelpText')} />

                              <FormInput
                                type={inputTypes.NUMBER}
                                name="minUpgradeFormatScore"
                                min={1}
                                {...minUpgradeFormatScore}
                                onChange={onInputChange}
                              />
                            </FormRow> :
                            null
                        }

                        <FormRow>
                          <FormLabel>
                            {translate('Language')}
                          </FormLabel>

                          <FormInputHelpText text={translate('LanguageHelpText')} />

                          <FormInput
                            type={inputTypes.LANGUAGE_SELECT}
                            name="language"
                            values={languages}
                            value={languageId}
                            onChange={onLanguageChange}
                          />
                        </FormRow>

                        <div className={styles.formatItemLarge}>
                          {getCustomFormatRender(formatItems, otherProps)}
                        </div>
                      </div>

                      <div className={styles.qualitiesColumn}>
                        <QualityProfileItems
                          editGroups={editGroups}
                          qualityProfileItems={items.value}
                          errors={items.errors}
                          warnings={items.warnings}
                          {...otherProps}
                        />
                      </div>

                      <div className={styles.formatItemSmall}>
                        {getCustomFormatRender(formatItems, otherProps)}
                      </div>
                    </div>
                  </Form>

              }
            </div>
          </Measure>
        </ModalBody>

        <Measure
          whitelist={['height']}
          includeMargin={false}
          onMeasure={this.onFooterMeasure}
        >
          <ModalFooter>
            {
              id ?
                <div
                  className={styles.deleteButtonContainer}
                  title={
                    isInUse ?
                      translate('QualityProfileInUseGameListCollection') :
                      undefined
                  }
                >
                  <Button
                    kind={kinds.DANGER}
                    isDisabled={isInUse}
                    onPress={onDeleteQualityProfilePress}
                  >
                    {translate('Delete')}
                  </Button>
                </div> :
                null
            }

            <Button
              onPress={onModalClose}
            >
              {translate('Cancel')}
            </Button>

            <SpinnerErrorButton
              isSpinning={isSaving}
              error={saveError}
              onPress={onSavePress}
            >
              {translate('Save')}
            </SpinnerErrorButton>
          </ModalFooter>
        </Measure>
      </ModalContent>
    );
  }
}

EditQualityProfileModalContent.propTypes = {
  editGroups: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFormats: PropTypes.arrayOf(PropTypes.object).isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  item: PropTypes.object.isRequired,
  isInUse: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onCutoffChange: PropTypes.func.isRequired,
  onLanguageChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onContentHeightChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteQualityProfilePress: PropTypes.func
};

export default EditQualityProfileModalContent;
