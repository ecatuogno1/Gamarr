// Sonarr v5 renamed FormInputGroup to FormInput and moved help text out of the
// input into FormRow, as FormInputHelpText siblings of the label. This alias
// keeps the old props working: help text is rendered as siblings before the
// input, which lands it in the label column of the v5 FormRow grid.
import React from 'react';
import { InputType } from 'Helpers/Props/inputTypes';
import translate from 'Utilities/String/translate';
import FormInput, { FormInputProps } from './FormInput';
import FormInputHelpText from './FormInputHelpText';

export type { ValidationMessage } from './FormInput';

export type FormInputGroupProps<V, C extends InputType> = FormInputProps<
  V,
  C
> & {
  helpText?: string;
  helpTexts?: string[];
  helpTextWarning?: string;
  helpLink?: string;
};

export interface FormInputGroupValues<T> {
  key: T;
  value: string;
  hint?: string;
}

function FormInputGroup<T, C extends InputType>(
  props: FormInputGroupProps<T, C>
) {
  const { helpText, helpTexts, helpTextWarning, helpLink, ...otherProps } =
    props;
  const isCheckInput = props.type === 'check';

  return (
    <>
      {helpText ? (
        <FormInputHelpText text={helpText} isCheckInput={isCheckInput} />
      ) : null}

      {helpTexts?.map((text, index) => (
        <FormInputHelpText
          key={index}
          text={text}
          isCheckInput={isCheckInput}
        />
      ))}

      {helpTextWarning ? (
        <FormInputHelpText text={helpTextWarning} isWarning={true} />
      ) : null}

      {helpLink ? (
        <FormInputHelpText text={translate('MoreInfo')} link={helpLink} />
      ) : null}

      <FormInput {...(otherProps as FormInputProps<T, C>)} />
    </>
  );
}

export default FormInputGroup;
