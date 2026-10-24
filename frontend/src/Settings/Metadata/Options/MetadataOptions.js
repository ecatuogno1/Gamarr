import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
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
    <FieldSet legend={translate('Options')}>
      {
        isFetching &&
          <LoadingIndicator />
      }

      {
        !isFetching && error &&
          <Alert kind={kinds.DANGER}>
            {translate('UnableToLoadIndexerOptions')}
          </Alert>
      }

      {
        hasSettings && !isFetching && !error &&
          <Form>
            <FormGroup>
              <FormLabel>{translate('IgdbClientId')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="igdbClientId"
                onChange={onInputChange}
                helpText={translate('IgdbClientIdHelpText')}
                helpLink="https://dev.twitch.tv/console/apps"
                {...settings.igdbClientId}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('IgdbClientSecret')}</FormLabel>

              <FormInputGroup
                type={inputTypes.PASSWORD}
                name="igdbClientSecret"
                onChange={onInputChange}
                helpText={translate('IgdbClientSecretHelpText')}
                {...settings.igdbClientSecret}
              />
            </FormGroup>
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
