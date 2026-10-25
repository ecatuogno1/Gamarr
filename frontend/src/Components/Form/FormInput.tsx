import classNames from 'classnames';
import React, { ElementType, ReactNode, useMemo, useState } from 'react';
import { inputTypes } from 'Helpers/Props';
import { InputType } from 'Helpers/Props/inputTypes';
import { ValidationError, ValidationWarning } from 'typings/pending';
import AutoCompleteInput, { AutoCompleteInputProps } from './AutoCompleteInput';
import CaptchaInput, { CaptchaInputProps } from './CaptchaInput';
import CheckInput, { CheckInputProps } from './CheckInput';
import FloatInput, { FloatInputProps } from './FloatInput';
import { FormInputButtonProps } from './FormInputButton';
import { FormInputGroupProvider } from './FormInputGroupContext';
import FormInputHelpText from './FormInputHelpText';
import { useFormRow } from './FormRowContext';
import KeyValueListInput, { KeyValueListInputProps } from './KeyValueListInput';
import NumberInput, { NumberInputProps } from './NumberInput';
import OAuthInput, { OAuthInputProps } from './OAuthInput';
import PasswordInput from './PasswordInput';
import PathInput, { PathInputProps } from './PathInput';
import AvailabilitySelectInput, {
  AvailabilitySelectInputProps,
} from './Select/AvailabilitySelectInput';
import DownloadClientSelectInput, {
  DownloadClientSelectInputProps,
} from './Select/DownloadClientSelectInput';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
} from './Select/EnhancedSelectInput';
import IndexerFlagsSelectInput, {
  IndexerFlagsSelectInputProps,
} from './Select/IndexerFlagsSelectInput';
import IndexerSelectInput, {
  IndexerSelectInputProps,
} from './Select/IndexerSelectInput';
import LanguageSelectInput, {
  LanguageSelectInputProps,
} from './Select/LanguageSelectInput';
import MonitorGamesSelectInput, {
  MonitorGamesSelectInputProps,
} from './Select/MonitorGamesSelectInput';
import PlatformSelectInput, {
  PlatformSelectInputProps,
} from './Select/PlatformSelectInput';
import ProviderDataSelectInput, {
  ProviderOptionSelectInputProps,
} from './Select/ProviderOptionSelectInput';
import QualityProfileSelectInput, {
  QualityProfileSelectInputProps,
} from './Select/QualityProfileSelectInput';
import RootFolderSelectInput, {
  RootFolderSelectInputProps,
} from './Select/RootFolderSelectInput';
import UMaskInput, { UMaskInputProps } from './Select/UMaskInput';
import DeviceInput, { DeviceInputProps } from './Tag/DeviceInput';
import GameTagInput, { GameTagInputProps } from './Tag/GameTagInput';
import TagSelectInput, { TagSelectInputProps } from './Tag/TagSelectInput';
import TextTagInput, { TextTagInputProps } from './Tag/TextTagInput';
import TextArea, { TextAreaProps } from './TextArea';
import TextInput, { TextInputProps } from './TextInput';
import styles from './FormInput.css';

const componentMap: Record<InputType, ElementType> = {
  autoComplete: AutoCompleteInput,
  availabilitySelect: AvailabilitySelectInput,
  captcha: CaptchaInput,
  check: CheckInput,
  date: TextInput,
  device: DeviceInput,
  downloadClientSelect: DownloadClientSelectInput,
  dynamicSelect: ProviderDataSelectInput,
  file: TextInput,
  float: FloatInput,
  indexerFlagsSelect: IndexerFlagsSelectInput,
  indexerSelect: IndexerSelectInput,
  keyValueList: KeyValueListInput,
  languageSelect: LanguageSelectInput,
  monitorGamesSelect: MonitorGamesSelectInput,
  gameTag: GameTagInput,
  number: NumberInput,
  oauth: OAuthInput,
  password: PasswordInput,
  path: PathInput,
  platformSelect: PlatformSelectInput,
  qualityProfileSelect: QualityProfileSelectInput,
  rootFolderSelect: RootFolderSelectInput,
  select: EnhancedSelectInput,
  tag: GameTagInput,
  tagSelect: TagSelectInput,
  text: TextInput,
  textArea: TextArea,
  textTag: TextTagInput,
  umask: UMaskInput,
} as const;

type PickProps<V, C extends InputType> = C extends 'autoComplete'
  ? AutoCompleteInputProps
  : C extends 'availabilitySelect'
  ? AvailabilitySelectInputProps
  : C extends 'captcha'
  ? CaptchaInputProps
  : C extends 'check'
  ? CheckInputProps
  : C extends 'date'
  ? TextInputProps
  : C extends 'device'
  ? DeviceInputProps
  : C extends 'downloadClientSelect'
  ? DownloadClientSelectInputProps
  : C extends 'dynamicSelect'
  ? ProviderOptionSelectInputProps
  : C extends 'file'
  ? TextInputProps
  : C extends 'float'
  ? FloatInputProps
  : C extends 'indexerFlagsSelect'
  ? IndexerFlagsSelectInputProps
  : C extends 'indexerSelect'
  ? IndexerSelectInputProps
  : C extends 'keyValueList'
  ? KeyValueListInputProps
  : C extends 'languageSelect'
  ? LanguageSelectInputProps
  : C extends 'monitorGamesSelect'
  ? MonitorGamesSelectInputProps
  : C extends 'gameTag'
  ? GameTagInputProps<V>
  : C extends 'number'
  ? NumberInputProps
  : C extends 'oauth'
  ? OAuthInputProps
  : C extends 'password'
  ? TextInputProps
  : C extends 'path'
  ? PathInputProps
  : C extends 'platformSelect'
  ? PlatformSelectInputProps
  : C extends 'qualityProfileSelect'
  ? QualityProfileSelectInputProps
  : C extends 'rootFolderSelect'
  ? RootFolderSelectInputProps
  : C extends 'select'
  ? // eslint-disable-next-line @typescript-eslint/no-explicit-any
    EnhancedSelectInputProps<any, V>
  : C extends 'tag'
  ? GameTagInputProps<V>
  : C extends 'tagSelect'
  ? TagSelectInputProps
  : C extends 'text'
  ? TextInputProps
  : C extends 'textArea'
  ? TextAreaProps
  : C extends 'textTag'
  ? TextTagInputProps
  : C extends 'umask'
  ? UMaskInputProps
  : never;

export interface ValidationMessage {
  message: string;
}

export type FormInputProps<V, C extends InputType> = Omit<
  PickProps<V, C>,
  'className'
> & {
  type: C;
  id?: string;
  className?: string;
  containerClassName?: string;
  inputClassName?: string;
  autoFocus?: boolean;
  autocomplete?: string;
  name: string;
  buttons?: ReactNode | ReactNode[];
  pending?: boolean;
  placeholder?: string;
  unit?: string;
  errors?: (ValidationMessage | ValidationError)[];
  warnings?: (ValidationMessage | ValidationWarning)[];
};

function FormInput<T, C extends InputType>(props: FormInputProps<T, C>) {
  const formRow = useFormRow();
  const {
    className = styles.inputGroup,
    containerClassName = styles.inputGroupContainer,
    inputClassName,
    type,
    id = formRow?.id ?? props.name,
    name,
    unit,
    buttons = [],
    pending,
    errors: serverErrors = [],
    warnings: serverWarnings = [],
    ...otherProps
  } = props;

  const [clientErrors, setClientErrors] = useState<
    (ValidationMessage | ValidationError)[]
  >([]);

  const [clientWarnings, setClientWarnings] = useState<
    (ValidationMessage | ValidationWarning)[]
  >([]);

  const errors = useMemo(() => {
    return [...clientErrors, ...serverErrors];
  }, [clientErrors, serverErrors]);

  const warnings = useMemo(() => {
    return [...clientWarnings, ...serverWarnings];
  }, [clientWarnings, serverWarnings]);

  const InputComponent = componentMap[type];
  const checkInput = type === inputTypes.CHECK;
  const hasError = !!errors.length;
  const hasWarning = !hasError && !!warnings.length;
  const describedBy = formRow?.descriptionIds.join(' ') || undefined;
  const buttonsArray = React.Children.toArray(buttons);
  const lastButtonIndex = buttonsArray.length - 1;
  const hasButton = !!buttonsArray.length;

  return (
    <FormInputGroupProvider
      describedBy={describedBy}
      hasError={hasError}
      setClientErrors={setClientErrors}
      setClientWarnings={setClientWarnings}
    >
      <div className={containerClassName}>
        <div
          className={classNames(
            className,
            checkInput && styles.checkInput,
            hasButton && styles.hasButtons,
            hasButton && hasError && styles.hasError,
            hasButton && hasWarning && styles.hasWarning
          )}
        >
          <div className={styles.inputContainer}>
            {/* @ts-expect-error - types are validated already */}
            <InputComponent
              id={id}
              name={name}
              className={inputClassName}
              hasError={hasError}
              hasWarning={hasWarning}
              hasButton={hasButton}
              {...otherProps}
            />

            {unit && (
              <div
                className={
                  type === inputTypes.NUMBER || type === inputTypes.FLOAT
                    ? styles.inputUnitNumber
                    : styles.inputUnit
                }
              >
                {unit}
              </div>
            )}
          </div>

          {buttonsArray.map((button, index) => {
            if (!React.isValidElement<FormInputButtonProps>(button)) {
              return button;
            }

            return React.cloneElement(button, {
              isLastButton: index === lastButtonIndex,
            });
          })}
        </div>

        {errors.map((error, index) => {
          return 'errorMessage' in error ? (
            <FormInputHelpText
              key={index}
              text={error.errorMessage}
              link={error.infoLink}
              tooltip={error.detailedDescription}
              isError={true}
              isCheckInput={checkInput}
            />
          ) : (
            <FormInputHelpText
              key={index}
              text={error.message}
              isError={true}
              isCheckInput={checkInput}
            />
          );
        })}

        {warnings.map((warning, index) => {
          return 'errorMessage' in warning ? (
            <FormInputHelpText
              key={index}
              text={warning.errorMessage}
              link={warning.infoLink}
              tooltip={warning.detailedDescription}
              isWarning={true}
              isCheckInput={checkInput}
            />
          ) : (
            <FormInputHelpText
              key={index}
              text={warning.message}
              isWarning={true}
              isCheckInput={checkInput}
            />
          );
        })}
      </div>
    </FormInputGroupProvider>
  );
}

export default FormInput;
