import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import { resetOAuth, startOAuth } from 'Store/Actions/oAuthActions';
import { InputOnChange } from 'typings/inputs';
import { useFormInputGroup } from './FormInputGroupContext';

export interface OAuthInputProps {
  id?: string;
  label?: string;
  name: string;
  provider: string;
  providerData: object;
  section: string;
  onChange: InputOnChange<unknown>;
}

function OAuthInput({
  id,
  label = 'Start OAuth',
  name,
  provider,
  providerData,
  section,
  onChange,
}: OAuthInputProps) {
  const dispatch = useDispatch();
  const formInputActions = useFormInputGroup();
  const { authorizing, error, result } = useSelector(
    (state: AppState) => state.oAuth
  );

  const handlePress = useCallback(() => {
    dispatch(
      startOAuth({
        name,
        provider,
        providerData,
        section,
      })
    );
  }, [name, provider, providerData, section, dispatch]);

  useEffect(() => {
    if (!result) {
      return;
    }

    Object.keys(result).forEach((key) => {
      onChange({ name: key, value: result[key] });
    });
  }, [result, onChange]);

  useEffect(() => {
    return () => {
      dispatch(resetOAuth());
    };
  }, [dispatch]);

  return (
    <div>
      <SpinnerErrorButton
        id={id}
        aria-describedby={formInputActions?.describedBy}
        aria-invalid={formInputActions?.hasError || undefined}
        isSpinning={authorizing}
        error={error}
        onPress={handlePress}
      >
        {label}
      </SpinnerErrorButton>
    </div>
  );
}

export default OAuthInput;
