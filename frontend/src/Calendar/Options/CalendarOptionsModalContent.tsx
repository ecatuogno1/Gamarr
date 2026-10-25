import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import {
  firstDayOfWeekOptions,
  timeFormatOptions,
  weekColumnOptions,
} from 'Settings/UI/UISettings';
import { setCalendarOption } from 'Store/Actions/calendarActions';
import { saveUISettings } from 'Store/Actions/settingsActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { InputChanged } from 'typings/inputs';
import UiSettings from 'typings/Settings/UiSettings';
import translate from 'Utilities/String/translate';

interface CalendarOptionsModalContentProps {
  onModalClose: () => void;
}

function CalendarOptionsModalContent({
  onModalClose,
}: CalendarOptionsModalContentProps) {
  const dispatch = useDispatch();

  const {
    showGameInformation,
    showReleaseDate,
    showCoverArt,
    showCutoffUnmetIcon,
    fullColorEvents,
  } = useSelector((state: AppState) => state.calendar.options);

  const uiSettings = useSelector(createUISettingsSelector());

  const [state, setState] = useState<Partial<UiSettings>>({
    firstDayOfWeek: uiSettings.firstDayOfWeek,
    calendarWeekColumnHeader: uiSettings.calendarWeekColumnHeader,
    timeFormat: uiSettings.timeFormat,
    enableColorImpairedMode: uiSettings.enableColorImpairedMode,
  });

  const {
    firstDayOfWeek,
    calendarWeekColumnHeader,
    timeFormat,
    enableColorImpairedMode,
  } = state;

  const handleOptionInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      dispatch(setCalendarOption({ [name]: value }));
    },
    [dispatch]
  );

  const handleGlobalInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      setState((prevState) => ({ ...prevState, [name]: value }));

      dispatch(saveUISettings({ [name]: value }));
    },
    [dispatch]
  );

  useEffect(() => {
    setState({
      firstDayOfWeek: uiSettings.firstDayOfWeek,
      calendarWeekColumnHeader: uiSettings.calendarWeekColumnHeader,
      timeFormat: uiSettings.timeFormat,
      enableColorImpairedMode: uiSettings.enableColorImpairedMode,
    });
  }, [uiSettings]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('CalendarOptions')}</ModalHeader>

      <ModalBody>
        <FieldSet legend={translate('Local')}>
          <Form>
            <FormRow>
              <FormLabel>{translate('ShowGameInformation')}</FormLabel>

              <FormInputHelpText
                text={translate('ShowGameInformationHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="showGameInformation"
                value={showGameInformation}
                onChange={handleOptionInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('ShowCoverArt')}</FormLabel>

              <FormInputHelpText text={translate('ShowCoverArtHelpText')} />
              <FormInput
                type={inputTypes.CHECK}
                name="showCoverArt"
                value={showCoverArt}
                onChange={handleOptionInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('ShowReleaseDate')}</FormLabel>

              <FormInputHelpText
                text={translate('ShowReleaseDateCalendarHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="showReleaseDate"
                value={showReleaseDate}
                onChange={handleOptionInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('IconForCutoffUnmet')}</FormLabel>

              <FormInputHelpText
                text={translate('IconForCutoffUnmetHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="showCutoffUnmetIcon"
                value={showCutoffUnmetIcon}
                onChange={handleOptionInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('FullColorEvents')}</FormLabel>

              <FormInputHelpText text={translate('FullColorEventsHelpText')} />
              <FormInput
                type={inputTypes.CHECK}
                name="fullColorEvents"
                value={fullColorEvents}
                onChange={handleOptionInputChange}
              />
            </FormRow>
          </Form>
        </FieldSet>

        <FieldSet legend={translate('Global')}>
          <Form>
            <FormRow>
              <FormLabel>{translate('FirstDayOfWeek')}</FormLabel>

              <FormInput
                type={inputTypes.SELECT}
                name="firstDayOfWeek"
                values={firstDayOfWeekOptions}
                value={firstDayOfWeek}
                onChange={handleGlobalInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('WeekColumnHeader')}</FormLabel>

              <FormInputHelpText text={translate('WeekColumnHeaderHelpText')} />
              <FormInput
                type={inputTypes.SELECT}
                name="calendarWeekColumnHeader"
                values={weekColumnOptions}
                value={calendarWeekColumnHeader}
                onChange={handleGlobalInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('TimeFormat')}</FormLabel>

              <FormInput
                type={inputTypes.SELECT}
                name="timeFormat"
                values={timeFormatOptions}
                value={timeFormat}
                onChange={handleGlobalInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('EnableColorImpairedMode')}</FormLabel>

              <FormInputHelpText
                text={translate('EnableColorImpairedModeHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="enableColorImpairedMode"
                value={enableColorImpairedMode}
                onChange={handleGlobalInputChange}
              />
            </FormRow>
          </Form>
        </FieldSet>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default CalendarOptionsModalContent;
