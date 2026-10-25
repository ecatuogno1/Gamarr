import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

function DownloadClientOptions(props) {
  const {
    advancedSettings,
    isFetching,
    error,
    settings,
    hasSettings,
    onInputChange
  } = props;

  return (
    <div>
      {
        isFetching ?
          <LoadingIndicator /> :
          null
      }

      {
        !isFetching && error ?
          <Alert kind={kinds.DANGER}>
            {translate('DownloadClientOptionsLoadError')}
          </Alert> :
          null
      }

      {
        hasSettings && !isFetching && !error && advancedSettings ?
          <div>
            <FieldSet
              legend={translate('CompletedDownloadHandling')}
              caption={translate('CompletedDownloadHandlingCaption')}
            >
              <Form>
                <FormRow
                  advancedSettings={advancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('Enable')}</FormLabel>

                  <FormInputHelpText
                    text={translate('EnableCompletedDownloadHandlingHelpText')}
                  />

                  <FormInput
                    type={inputTypes.CHECK}
                    name="enableCompletedDownloadHandling"
                    onChange={onInputChange}
                    {...settings.enableCompletedDownloadHandling}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={advancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('CheckForFinishedDownloadsInterval')}</FormLabel>

                  <FormInputHelpText
                    text={translate('RefreshMonitoredIntervalHelpText')}
                  />

                  <FormInput
                    type={inputTypes.NUMBER}
                    name="checkForFinishedDownloadInterval"
                    min={1}
                    max={120}
                    unit="minutes"
                    onChange={onInputChange}
                    {...settings.checkForFinishedDownloadInterval}
                  />
                </FormRow>
              </Form>
            </FieldSet>

            <FieldSet
              legend={translate('FailedDownloadHandling')}
              caption={translate('FailedDownloadHandlingCaption')}
              notice={
                <Alert kind={kinds.INFO}>
                  {translate('RemoveDownloadsAlert')}
                </Alert>
              }
            >
              <Form>
                <FormRow
                  advancedSettings={advancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('AutoRedownloadFailed')}</FormLabel>

                  <FormInputHelpText
                    text={translate('AutoRedownloadFailedHelpText')}
                  />

                  <FormInput
                    type={inputTypes.CHECK}
                    name="autoRedownloadFailed"
                    onChange={onInputChange}
                    {...settings.autoRedownloadFailed}
                  />
                </FormRow>

                {
                  settings.autoRedownloadFailed.value ?
                    <FormRow
                      advancedSettings={advancedSettings}
                      isAdvanced={true}
                    >
                      <FormLabel>{translate('AutoRedownloadFailedFromInteractiveSearch')}</FormLabel>

                      <FormInputHelpText
                        text={translate('AutoRedownloadFailedFromInteractiveSearchHelpText')}
                      />

                      <FormInput
                        type={inputTypes.CHECK}
                        name="autoRedownloadFailedFromInteractiveSearch"
                        onChange={onInputChange}
                        {...settings.autoRedownloadFailedFromInteractiveSearch}
                      />
                    </FormRow> :
                    null
                }
              </Form>
            </FieldSet>
          </div> :
          null
      }
    </div>
  );
}

DownloadClientOptions.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default DownloadClientOptions;
