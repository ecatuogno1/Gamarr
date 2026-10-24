import React, { useCallback, useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { EnhancedSelectInputChanged } from 'typings/inputs';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

export interface PlatformSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<
      EnhancedSelectInputValue<number | string>,
      number | string
    >,
    'values'
  > {
  name: string;
  includeAny?: boolean;
  includeNoChange?: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed?: boolean;
  // Platform ids that can't be picked (e.g. the game is already in the library on them)
  disabledPlatformIds?: number[];
}

// Enabled platforms (plus the selected one if it was disabled since), sorted by name
function PlatformSelectInput({
  name,
  value,
  includeAny = false,
  includeNoChange = false,
  includeNoChangeDisabled = true,
  includeMixed = false,
  disabledPlatformIds = [],
  onChange,
  ...otherProps
}: PlatformSelectInputProps) {
  const platforms = useSelector(
    (state: AppState) => state.settings.platforms.items
  );

  const values = useMemo(() => {
    const options: EnhancedSelectInputValue<number | string>[] = platforms
      .filter((platform) => platform.enabled || platform.id === value)
      .sort(sortByProp('name'))
      .map((platform) => ({
        key: platform.id,
        value: platform.name,
        hint: platform.shortName,
        isDisabled: disabledPlatformIds.includes(platform.id),
      }));

    if (includeAny) {
      options.unshift({ key: 0, value: translate('AnyPlatform') });
    }

    if (includeNoChange) {
      options.unshift({
        key: 'noChange',
        value: translate('NoChange'),
        isDisabled: includeNoChangeDisabled,
      });
    }

    if (includeMixed) {
      options.unshift({
        key: 'mixed',
        value: `(${translate('Mixed')})`,
        isDisabled: true,
      });
    }

    return options;
  }, [
    platforms,
    value,
    includeAny,
    includeNoChange,
    includeNoChangeDisabled,
    includeMixed,
    disabledPlatformIds,
  ]);

  const handleChange = useCallback(
    ({ value: newValue }: EnhancedSelectInputChanged<string | number>) => {
      onChange({ name, value: newValue });
    },
    [name, onChange]
  );

  return (
    <EnhancedSelectInput
      {...otherProps}
      name={name}
      value={value}
      values={values}
      onChange={handleChange}
    />
  );
}

export default PlatformSelectInput;
