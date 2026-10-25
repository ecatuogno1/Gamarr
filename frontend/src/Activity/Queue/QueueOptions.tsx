import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { inputTypes } from 'Helpers/Props';
import { gotoQueuePage, setQueueOption } from 'Store/Actions/queueActions';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

function QueueOptions() {
  const dispatch = useDispatch();
  const { includeUnknownGameItems } = useSelector(
    (state: AppState) => state.queue.options
  );

  const handleOptionChange = useCallback(
    ({ name, value }: InputChanged<boolean>) => {
      dispatch(
        setQueueOption({
          [name]: value,
        })
      );

      if (name === 'includeUnknownGameItems') {
        dispatch(gotoQueuePage({ page: 1 }));
      }
    },
    [dispatch]
  );

  return (
    <FormRow>
      <FormLabel>{translate('ShowUnknownGameItems')}</FormLabel>

      <FormInputHelpText text={translate('ShowUnknownGameItemsHelpText')} />

      <FormInput
        type={inputTypes.CHECK}
        name="includeUnknownGameItems"
        value={includeUnknownGameItems}
        onChange={handleOptionChange}
      />
    </FormRow>
  );
}

export default QueueOptions;
