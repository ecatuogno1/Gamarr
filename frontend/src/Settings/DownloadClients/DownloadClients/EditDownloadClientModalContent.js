import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import { inputTypes, kinds } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import translate from 'Utilities/String/translate';
import styles from './EditDownloadClientModalContent.css';

class EditDownloadClientModalContent extends Component {

  //
  // Render

  render() {
    const {
      advancedSettings,
      isFetching,
      error,
      isSaving,
      isTesting,
      saveError,
      item,
      onInputChange,
      onFieldChange,
      onModalClose,
      onSavePress,
      onTestPress,
      onDeleteDownloadClientPress,
      ...otherProps
    } = this.props;

    const {
      id,
      implementationName,
      name,
      enable,
      priority,
      removeCompletedDownloads,
      removeFailedDownloads,
      fields,
      tags,
      message
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader subtitle={id ? name.previousValue ?? name.value : undefined}>
          {id ? translate('EditDownloadClientImplementation', { implementationName }) : translate('AddDownloadClientImplementation', { implementationName })}
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <Alert kind={kinds.DANGER}>
                {translate('AddDownloadClientError')}
              </Alert>
          }

          {
            !isFetching && !error &&
              <Form {...otherProps}>
                {
                  !!message &&
                    <Alert
                      className={styles.message}
                      kind={message.value.type}
                    >
                      {message.value.message}
                    </Alert>
                }

                <FormRow>
                  <FormLabel>{translate('Name')}</FormLabel>

                  <FormInput
                    type={inputTypes.TEXT}
                    name="name"
                    {...name}
                    onChange={onInputChange}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>{translate('Enable')}</FormLabel>

                  <FormInput
                    type={inputTypes.CHECK}
                    name="enable"
                    {...enable}
                    onChange={onInputChange}
                  />
                </FormRow>

                {
                  fields.map((field) => {
                    return (
                      <ProviderFieldFormGroup
                        key={field.name}
                        advancedSettings={advancedSettings}
                        provider="downloadClient"
                        providerData={item}
                        {...field}
                        onChange={onFieldChange}
                      />
                    );
                  })
                }

                <FormRow
                  advancedSettings={advancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('ClientPriority')}</FormLabel>

                  <FormInputHelpText text={translate('DownloadClientPriorityHelpText')} />

                  <FormInput
                    type={inputTypes.NUMBER}
                    name="priority"
                    min={1}
                    max={50}
                    {...priority}
                    onChange={onInputChange}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>{translate('Tags')}</FormLabel>

                  <FormInputHelpText text={translate('DownloadClientGameTagHelpText')} />

                  <FormInput
                    type={inputTypes.TAG}
                    name="tags"
                    {...tags}
                    onChange={onInputChange}
                  />
                </FormRow>

                <ModalSection title={translate('CompletedDownloadHandling')}>
                  <FormRow>
                    <FormLabel>{translate('RemoveCompleted')}</FormLabel>

                    <FormInputHelpText text={translate('RemoveCompletedDownloadsHelpText')} />

                    <FormInput
                      type={inputTypes.CHECK}
                      name="removeCompletedDownloads"
                      {...removeCompletedDownloads}
                      onChange={onInputChange}
                    />
                  </FormRow>

                  <FormRow>
                    <FormLabel>{translate('RemoveFailed')}</FormLabel>

                    <FormInputHelpText text={translate('RemoveFailedDownloadsHelpText')} />

                    <FormInput
                      type={inputTypes.CHECK}
                      name="removeFailedDownloads"
                      {...removeFailedDownloads}
                      onChange={onInputChange}
                    />
                  </FormRow>
                </ModalSection>
              </Form>
          }
        </ModalBody>
        <ModalFooter>
          {
            id &&
              <Button
                className={styles.deleteButton}
                kind={kinds.DANGER}
                onPress={onDeleteDownloadClientPress}
              >
                {translate('Delete')}
              </Button>
          }

          <AdvancedSettingsButton
            showLabel={false}
          />

          <SpinnerErrorButton
            isSpinning={isTesting}
            error={saveError}
            onPress={onTestPress}
          >
            {translate('Test')}
          </SpinnerErrorButton>

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
      </ModalContent>
    );
  }
}

EditDownloadClientModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  isTesting: PropTypes.bool.isRequired,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onTestPress: PropTypes.func.isRequired,
  onDeleteDownloadClientPress: PropTypes.func
};

export default EditDownloadClientModalContent;
