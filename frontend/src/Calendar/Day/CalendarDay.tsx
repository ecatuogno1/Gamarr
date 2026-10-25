import classNames from 'classnames';
import moment from 'moment';
import React from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import * as calendarViews from 'Calendar/calendarViews';
import CalendarEvent from 'Calendar/Events/CalendarEvent';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import { CalendarEvent as CalendarEventModel } from 'typings/Calendar';
import translate from 'Utilities/String/translate';
import styles from './CalendarDay.css';

function sort(items: CalendarEventModel[]) {
  return items.sort((a, b) => {
    const aDate = moment(a.releaseDate).unix();
    const bDate = moment(b.releaseDate).unix();

    return aDate - bDate;
  });
}

function createCalendarEventsConnector(date: string) {
  return createSelector(
    (state: AppState) => state.calendar.items,
    (state: AppState) => state.calendar.options,
    (items, options) => {
      const { showReleaseDate } = options;
      const momentDate = moment(date);

      // Games have one release date (on the game's platform)
      const filtered = items.filter(({ releaseDate }) => {
        return (
          showReleaseDate &&
          releaseDate &&
          momentDate.isSame(moment(releaseDate), 'day')
        );
      });

      return sort(filtered);
    }
  );
}

interface CalendarDayProps {
  date: string;
  isTodaysDate: boolean;
}

function CalendarDay({ date, isTodaysDate }: CalendarDayProps) {
  const { time, view } = useSelector((state: AppState) => state.calendar);
  const events = useSelector(createCalendarEventsConnector(date));
  const { longDateFormat } = useUiSettingsValues();
  const momentDate = moment(date);

  const ref = React.useRef<HTMLTableCellElement>(null);

  React.useEffect(() => {
    if (isTodaysDate && view === calendarViews.MONTH && ref.current) {
      ref.current.scrollIntoView();
    }
  }, [time, isTodaysDate, view]);

  return (
    <td
      ref={ref}
      className={classNames(
        styles.day,
        view === calendarViews.DAY && styles.isSingleDay,
        isTodaysDate && styles.isTodayCell
      )}
    >
      {view === calendarViews.MONTH && (
        <div
          className={classNames(
            styles.dayOfMonth,
            isTodaysDate && styles.isToday,
            !moment(date).isSame(moment(time), 'month') &&
              styles.isDifferentMonth
          )}
        >
          <time
            dateTime={momentDate.format('YYYY-MM-DD')}
            aria-current={isTodaysDate ? 'date' : undefined}
          >
            <span className={styles.fullDate}>
              {momentDate.format(longDateFormat)}
            </span>
            <span aria-hidden={true}>{momentDate.date()}</span>
          </time>
        </div>
      )}
      {!events.length && view === calendarViews.DAY ? (
        <div className={styles.noEvents}>{translate('NoGameReleases')}</div>
      ) : null}
      <div>
        {events.map((event) => {
          return (
            <CalendarEvent key={event.id} {...event} date={date as string} />
          );
        })}
      </div>
    </td>
  );
}

export default CalendarDay;
