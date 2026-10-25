import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import { inputTypes, kinds } from 'Helpers/Props';
import { boolSettingShape, numberSettingShape, tagSettingShape } from 'Helpers/Props/Shapes/settingShape';
import translate from 'Utilities/String/translate';
import styles from './EditDelayProfileModalContent.css';

const protocolOptions = [
  {
    key: 'preferUsenet',
    get value() {
      return translate('PreferUsenet');
    }
  },
  {
    key: 'preferTorrent',
    get value() {
      return translate('PreferTorrent');
    }
  },
  {
    key: 'onlyUsenet',
    get value() {
      return translate('OnlyUsenet');
    }
  },
  {
    key: 'onlyTorrent',
    get value() {
      return translate('OnlyTorrent');
    }
  }
];

function EditDelayProfileModalContent(props) {
  const {
    id,
    isFetching,
    error,
    isSaving,
    saveError,
    item,
    protocol,
    onInputChange,
    onProtocolChange,
    onSavePress,
    onModalClose,
    onDeleteDelayProfilePress,
    ...otherProps
  } = props;

  const {
    enableUsenet,
    enableTorrent,
    usenetDelay,
    torrentDelay,
    bypassIfHighestQuality,
    bypassIfAboveCustomFormatScore,
    minimumCustomFormatScore,
    tags
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditDelayProfile') : translate('AddDelayProfile')}
      </ModalHeader>

      <ModalBody>
        {
          isFetching ?
            <LoadingIndicator /> :
            null
        }

        {
          !isFetching && !!error ?
            <Alert kind={kinds.DANGER}>
              {translate('AddDelayProfileError')}
            </Alert> :
            null
        }

        {
          !isFetching && !error ?
            <Form {...otherProps}>
              <ModalSection title={translate('Protocol')}>
                <FormRow>
                  <FormLabel>{translate('PreferredProtocol')}</FormLabel>

                  <FormInputHelpText text={translate('ProtocolHelpText')} />

                  <FormInput
                    type={inputTypes.SELECT}
                    name="protocol"
                    value={protocol}
                    values={protocolOptions}
                    onChange={onProtocolChange}
                  />
                </FormRow>

                {
                  enableUsenet.value ?
                    <FormRow>
                      <FormLabel>{translate('UsenetDelay')}</FormLabel>

                      <FormInputHelpText text={translate('UsenetDelayHelpText')} />

                      <FormInput
                        type={inputTypes.NUMBER}
                        name="usenetDelay"
                        unit="minutes"
                        {...usenetDelay}
                        onChange={onInputChange}
                      />
                    </FormRow> :
                    null
                }

                {
                  enableTorrent.value ?
                    <FormRow>
                      <FormLabel>{translate('TorrentDelay')}</FormLabel>

                      <FormInputHelpText text={translate('TorrentDelayHelpText')} />

                      <FormInput
                        type={inputTypes.NUMBER}
                        name="torrentDelay"
                        unit="minutes"
                        {...torrentDelay}
                        onChange={onInputChange}
                      />
                    </FormRow> :
                    null
                }
              </ModalSection>

              <ModalSection title={translate('ProfileSectionBypass')}>
                <FormRow>
                  <FormLabel>{translate('BypassDelayIfHighestQuality')}</FormLabel>

                  <FormInputHelpText text={translate('BypassDelayIfHighestQualityHelpText')} />

                  <FormInput
                    type={inputTypes.CHECK}
                    name="bypassIfHighestQuality"
                    {...bypassIfHighestQuality}
                    onChange={onInputChange}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>{translate('BypassDelayIfAboveCustomFormatScore')}</FormLabel>

                  <FormInputHelpText text={translate('BypassDelayIfAboveCustomFormatScoreHelpText')} />

                  <FormInput
                    type={inputTypes.CHECK}
                    name="bypassIfAboveCustomFormatScore"
                    {...bypassIfAboveCustomFormatScore}
                    onChange={onInputChange}
                  />
                </FormRow>

                {
                  bypassIfAboveCustomFormatScore.value ?
                    <FormRow>
                      <FormLabel>{translate('BypassDelayIfAboveCustomFormatScoreMinimumScore')}</FormLabel>

                      <FormInputHelpText text={translate('BypassDelayIfAboveCustomFormatScoreMinimumScoreHelpText')} />

                      <FormInput
                        type={inputTypes.NUMBER}
                        name="minimumCustomFormatScore"
                        {...minimumCustomFormatScore}
                        onChange={onInputChange}
                      />
                    </FormRow> :
                    null
                }
              </ModalSection>

              {
                id === 1 ?
                  <Alert className={styles.intro}>
                    {translate('DefaultDelayProfileGame')}
                  </Alert> :

                  <FormRow>
                    <FormLabel>{translate('Tags')}</FormLabel>

                    <FormInputHelpText text={translate('DelayProfileGameTagsHelpText')} />

                    <FormInput
                      type={inputTypes.TAG}
                      name="tags"
                      {...tags}
                      onChange={onInputChange}
                    />
                  </FormRow>
              }
            </Form> :
            null
        }
      </ModalBody>
      <ModalFooter>
        {
          id && id > 1 ?
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteDelayProfilePress}
            >
              {translate('Delete')}
            </Button> :
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
    </ModalContent>
  );
}

const delayProfileShape = {
  enableUsenet: PropTypes.shape(boolSettingShape).isRequired,
  enableTorrent: PropTypes.shape(boolSettingShape).isRequired,
  usenetDelay: PropTypes.shape(numberSettingShape).isRequired,
  torrentDelay: PropTypes.shape(numberSettingShape).isRequired,
  bypassIfHighestQuality: PropTypes.shape(boolSettingShape).isRequired,
  bypassIfAboveCustomFormatScore: PropTypes.shape(boolSettingShape).isRequired,
  minimumCustomFormatScore: PropTypes.shape(numberSettingShape).isRequired,
  order: PropTypes.shape(numberSettingShape),
  tags: PropTypes.shape(tagSettingShape).isRequired
};

EditDelayProfileModalContent.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.shape(delayProfileShape).isRequired,
  protocol: PropTypes.string.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onProtocolChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteDelayProfilePress: PropTypes.func
};

export default EditDelayProfileModalContent;
