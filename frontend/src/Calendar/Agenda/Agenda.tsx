import moment from 'moment';
import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Game from 'Game/Game';
import AgendaEvent from './AgendaEvent';
import styles from './Agenda.css';

interface AgendaGame extends Game {
  sortDate: moment.Moment;
}

function Agenda() {
  const { start, end, items } = useSelector(
    (state: AppState) => state.calendar
  );

  const events = useMemo(() => {
    const result = items.map((item): AgendaGame => {
      const { releaseDate } = item;

      const dates = [];

      if (releaseDate) {
        const releaseDateMoment = moment(releaseDate);

        if (
          releaseDateMoment.isAfter(start) &&
          releaseDateMoment.isBefore(end)
        ) {
          dates.push(releaseDateMoment);
        }
      }

      const sortDate = moment.min(...dates);

      return {
        ...item,
        sortDate,
      };
    });

    result.sort((a, b) => (a.sortDate > b.sortDate ? 1 : -1));

    return result;
  }, [items, start, end]);

  return (
    <div className={styles.agenda}>
      {events.map((item, index) => {
        const momentDate = moment(item.sortDate);
        const showDate =
          index === 0 ||
          !moment(events[index - 1].sortDate).isSame(momentDate, 'day');

        return <AgendaEvent key={item.id} showDate={showDate} {...item} />;
      })}
    </div>
  );
}

export default Agenda;
