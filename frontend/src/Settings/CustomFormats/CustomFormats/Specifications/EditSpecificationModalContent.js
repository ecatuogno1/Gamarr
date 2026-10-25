import PropTypes from 'prop-types';
import React from 'react';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './EditSpecificationModalContent.css';

function EditSpecificationModalContent(props) {
  const {
    advancedSettings,
    item,
    onInputChange,
    onFieldChange,
    onCancelPress,
    onSavePress,
    onDeleteSpecificationPress,
    ...otherProps
  } = props;

  const {
    id,
    implementationName,
    name,
    negate,
    required,
    fields
  } = item;

  return (
    <ModalContent onModalClose={onCancelPress}>
      <ModalHeader subtitle={id ? name.value : undefined}>
        {id ? translate('EditConditionImplementation', { implementationName }) : translate('AddConditionImplementation', { implementationName })}
      </ModalHeader>

      <ModalBody>
        <Form
          {...otherProps}
        >
          {
            fields && fields.some((x) => x.label === translate('CustomFormatsSpecificationRegularExpression')) ?
              <p className={styles.intro}>
                <InlineMarkdown data={translate('ConditionUsingRegularExpressions')} />{' '}
                <InlineMarkdown data={translate('RegularExpressionsTutorialLink', { url: 'https://www.regular-expressions.info/tutorial.html' })} />{' '}
                <InlineMarkdown data={translate('RegularExpressionsCanBeTested', { url: 'http://regexstorm.net/tester' })} />
              </p> :
              null
          }

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

          {
            fields && fields.map((field) => {
              return (
                <ProviderFieldFormGroup
                  key={field.name}
                  advancedSettings={advancedSettings}
                  provider="specifications"
                  providerData={item}
                  {...field}
                  onChange={onFieldChange}
                />
              );
            })
          }

          <FormRow>
            <FormLabel>
              {translate('Negate')}
            </FormLabel>
            <FormInputHelpText
              text={translate('NegateHelpText', { implementationName })}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="negate"
              {...negate}
              onChange={onInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>
              {translate('Required')}
            </FormLabel>
            <FormInputHelpText
              text={translate('RequiredHelpText', { implementationName })}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="required"
              {...required}
              onChange={onInputChange}
            />
          </FormRow>
        </Form>
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteSpecificationPress}
            >
              {translate('Delete')}
            </Button>
        }

        <Button
          onPress={onCancelPress}
        >
          {translate('Cancel')}
        </Button>

        <SpinnerErrorButton
          isSpinning={false}
          onPress={onSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditSpecificationModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onCancelPress: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onDeleteSpecificationPress: PropTypes.func
};

export default EditSpecificationModalContent;
