import moment from 'moment';
import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import * as calendarViews from 'Calendar/calendarViews';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import translate from 'Utilities/String/translate';
import styles from './CalendarTableHeader.css';

interface CalendarTableHeaderProps {
  todaysDate: string;
}

function CalendarTableHeader({ todaysDate }: CalendarTableHeaderProps) {
  const { dates, view } = useSelector((state: AppState) => state.calendar);
  const { longDateFormat } = useUiSettingsValues();
  const headerDates = view === calendarViews.MONTH ? dates.slice(0, 7) : dates;

  return (
    <thead>
      <tr className={styles.headerRow}>
        {headerDates.map((date) => {
          const momentDate = moment(date);
          const isTodaysDate =
            view !== calendarViews.MONTH && date === todaysDate;
          const label = momentDate.format(
            view === calendarViews.MONTH ? 'dddd' : longDateFormat
          );

          return (
            <th key={date} className={styles.headerCell} scope="col">
              <time
                className={styles.headerLabel}
                dateTime={momentDate.format('YYYY-MM-DD')}
                aria-current={isTodaysDate ? 'date' : undefined}
              >
                {isTodaysDate ? `${label}, ${translate('Today')}` : label}
              </time>
            </th>
          );
        })}
      </tr>
    </thead>
  );
}

export default CalendarTableHeader;
