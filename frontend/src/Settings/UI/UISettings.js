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
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import { inputTypes, kinds } from 'Helpers/Props';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

export const firstDayOfWeekOptions = [
  {
    key: 0,
    get value() {
      return translate('Sunday');
    }
  },
  {
    key: 1,
    get value() {
      return translate('Monday');
    }
  }
];

export const weekColumnOptions = [
  { key: 'ddd M/D', value: 'Tue 3/25', hint: 'ddd M/D' },
  { key: 'ddd MM/DD', value: 'Tue 03/25', hint: 'ddd MM/DD' },
  { key: 'ddd D/M', value: 'Tue 25/3', hint: 'ddd D/M' },
  { key: 'ddd DD/MM', value: 'Tue 25/03', hint: 'ddd DD/MM' }
];

const shortDateFormatOptions = [
  { key: 'MMM D YYYY', value: 'Mar 25 2014', hint: 'MMM D YYYY' },
  { key: 'DD MMM YYYY', value: '25 Mar 2014', hint: 'DD MMM YYYY' },
  { key: 'MM/D/YYYY', value: '03/25/2014', hint: 'MM/D/YYYY' },
  { key: 'MM/DD/YYYY', value: '03/25/2014', hint: 'MM/DD/YYYY' },
  { key: 'DD/MM/YYYY', value: '25/03/2014', hint: 'DD/MM/YYYY' },
  { key: 'YYYY-MM-DD', value: '2014-03-25', hint: 'YYYY-MM-DD' }
];

const longDateFormatOptions = [
  { key: 'dddd, MMMM D YYYY', value: 'Tuesday, March 25, 2014' },
  { key: 'dddd, D MMMM YYYY', value: 'Tuesday, 25 March, 2014' }
];

export const timeFormatOptions = [
  { key: 'h(:mm)a', value: '5pm/5:30pm' },
  { key: 'HH:mm', value: '17:00/17:30' }
];

const headerColorOptions = [
  {
    key: 'fullColor',
    get value() {
      return translate('FullColor');
    }
  },
  {
    key: 'gradient',
    get value() {
      return translate('Gradient');
    }
  }
];

function UISettings(props) {
  const {
    isFetching,
    isSaving,
    error,
    settings,
    hasSettings,
    hasPendingChanges,
    validationErrors,
    validationWarnings,
    languages,
    onInputChange,
    onSavePress
  } = props;

  // Must stay in sync with the theme selectors in Styles/Themes/themes.css.
  const themeOptions = ['auto', 'light', 'dark']
    .map((theme) => ({ key: theme, value: titleCase(theme) }));

  const uiLanguages = languages.filter((item) => item.value !== 'Original');

  return (
    <SettingsPage
      title={translate('UiSettings')}
      hasPendingChanges={hasPendingChanges}
      isSaving={isSaving}
      onSavePress={onSavePress}
    >
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading scope={translate('Settings')} title={translate('Ui')} />

          {
            isFetching ?
              <LoadingIndicator /> :
              null
          }

          {
            !isFetching && error ?
              <Alert kind={kinds.DANGER}>
                {translate('UiSettingsLoadError')}
              </Alert> :
              null
          }

          {
            hasSettings && !isFetching && !error ?
              <Form
                id="uiSettings"
                validationErrors={validationErrors}
                validationWarnings={validationWarnings}
              >
                <FieldSet
                  legend={translate('Calendar')}
                  caption={translate('CalendarCaption')}
                >
                  <FormRow>
                    <FormLabel>{translate('FirstDayOfWeek')}</FormLabel>
                    <FormInput
                      type={inputTypes.SELECT}
                      name="firstDayOfWeek"
                      values={firstDayOfWeekOptions}
                      onChange={onInputChange}
                      {...settings.firstDayOfWeek}
                    />
                  </FormRow>

                  <FormRow>
                    <FormLabel>{translate('WeekColumnHeader')}</FormLabel>
                    <FormInputHelpText text={translate('WeekColumnHeaderHelpText')} />
                    <FormInput
                      type={inputTypes.SELECT}
                      name="calendarWeekColumnHeader"
                      values={weekColumnOptions}
                      onChange={onInputChange}
                      {...settings.calendarWeekColumnHeader}
                    />
                  </FormRow>
                </FieldSet>

                <FieldSet
                  legend={translate('Dates')}
                  caption={translate('DatesCaption')}
                >
                  <FormRow>
                    <FormLabel>{translate('ShortDateFormat')}</FormLabel>
                    <FormInput
                      type={inputTypes.SELECT}
                      name="shortDateFormat"
                      values={shortDateFormatOptions}
                      onChange={onInputChange}
                      {...settings.shortDateFormat}
                    />
                  </FormRow>

                  <FormRow>
                    <FormLabel>{translate('LongDateFormat')}</FormLabel>
                    <FormInput
                      type={inputTypes.SELECT}
                      name="longDateFormat"
                      values={longDateFormatOptions}
                      onChange={onInputChange}
                      {...settings.longDateFormat}
                    />
                  </FormRow>

                  <FormRow>
                    <FormLabel>{translate('TimeFormat')}</FormLabel>
                    <FormInput
                      type={inputTypes.SELECT}
                      name="timeFormat"
                      values={timeFormatOptions}
                      onChange={onInputChange}
                      {...settings.timeFormat}
                    />
                  </FormRow>

                  <FormRow>
                    <FormLabel>{translate('ShowRelativeDates')}</FormLabel>
                    <FormInputHelpText text={translate('ShowRelativeDatesHelpText')} />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="showRelativeDates"
                      onChange={onInputChange}
                      {...settings.showRelativeDates}
                    />
                  </FormRow>
                </FieldSet>

                <FieldSet
                  legend={translate('Style')}
                  caption={translate('StyleCaption')}
                >
                  <FormRow>
                    <FormLabel>{translate('Theme')}</FormLabel>
                    <FormInputHelpText text={translate('ThemeHelpText')} />
                    <FormInput
                      type={inputTypes.SELECT}
                      name="theme"
                      values={themeOptions}
                      onChange={onInputChange}
                      {...settings.theme}
                    />
                  </FormRow>

                  <FormRow>
                    <FormLabel>{translate('HeaderColor')}</FormLabel>
                    <FormInputHelpText text={translate('HeaderColorHelpText')} />
                    <FormInput
                      type={inputTypes.SELECT}
                      name="headerColor"
                      values={headerColorOptions}
                      onChange={onInputChange}
                      {...settings.headerColor}
                    />
                  </FormRow>

                  <FormRow>
                    <FormLabel>{translate('EnableColorImpairedMode')}</FormLabel>
                    <FormInputHelpText text={translate('EnableColorImpairedModeHelpText')} />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="enableColorImpairedMode"
                      onChange={onInputChange}
                      {...settings.enableColorImpairedMode}
                    />
                  </FormRow>
                </FieldSet>

                <FieldSet
                  legend={translate('Language')}
                  caption={translate('LanguageCaption')}
                >
                  <FormRow>
                    <FormLabel>{translate('GameInfoLanguage')}</FormLabel>
                    <FormInputHelpText text={translate('GameInfoLanguageHelpText')} />
                    <FormInputHelpText
                      text={translate('GameInfoLanguageHelpTextWarning')}
                      isWarning={true}
                    />
                    <FormInput
                      type={inputTypes.LANGUAGE_SELECT}
                      name="gameInfoLanguage"
                      values={languages}
                      onChange={onInputChange}
                      {...settings.gameInfoLanguage}
                      errors={
                        languages.some((language) => language.key === settings.gameInfoLanguage.value) ?
                          settings.gameInfoLanguage.errors :
                          [
                            ...settings.gameInfoLanguage.errors,
                            { message: translate('InvalidGameInfoLanguageLanguage') }
                          ]}
                    />
                  </FormRow>

                  <FormRow>
                    <FormLabel>{translate('UiLanguage')}</FormLabel>
                    <FormInputHelpText text={translate('UiLanguageHelpText')} />
                    <FormInputHelpText
                      text={translate('BrowserReloadRequired')}
                      isWarning={true}
                    />
                    <FormInput
                      type={inputTypes.LANGUAGE_SELECT}
                      name="uiLanguage"
                      values={uiLanguages}
                      onChange={onInputChange}
                      {...settings.uiLanguage}
                      errors={
                        languages.some((language) => language.key === settings.uiLanguage.value) ?
                          settings.uiLanguage.errors :
                          [
                            ...settings.uiLanguage.errors,
                            { message: translate('InvalidUILanguage') }
                          ]}
                    />
                  </FormRow>
                </FieldSet>
              </Form> :
              null
          }
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

UISettings.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool,
  validationErrors: PropTypes.arrayOf(PropTypes.object),
  validationWarnings: PropTypes.arrayOf(PropTypes.object),
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSavePress: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default UISettings;
