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

function MetadataOptions(props) {
  const {
    isFetching,
    error,
    settings,
    hasSettings,
    onInputChange
  } = props;

  return (
    <FieldSet
      legend={translate('Options')}
      caption={translate('MetadataOptionsCaption')}
    >
      {
        isFetching &&
          <LoadingIndicator />
      }

      {
        !isFetching && error &&
          <Alert kind={kinds.DANGER}>
            {translate('MetadataOptionsLoadError')}
          </Alert>
      }

      {
        hasSettings && !isFetching && !error &&
          <Form>
            <FormRow>
              <FormLabel>{translate('IgdbClientId')}</FormLabel>
              <FormInputHelpText
                text={translate('IgdbClientIdHelpText')}
                link="https://dev.twitch.tv/console/apps"
              />
              <FormInput
                type={inputTypes.TEXT}
                name="igdbClientId"
                onChange={onInputChange}
                {...settings.igdbClientId}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('IgdbClientSecret')}</FormLabel>
              <FormInputHelpText
                text={translate('IgdbClientSecretHelpText')}
              />
              <FormInput
                type={inputTypes.PASSWORD}
                name="igdbClientSecret"
                onChange={onInputChange}
                {...settings.igdbClientSecret}
              />
            </FormRow>
          </Form>
      }
    </FieldSet>
  );
}

MetadataOptions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default MetadataOptions;
