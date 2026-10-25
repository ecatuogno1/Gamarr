import classNames from 'classnames';
import React from 'react';
import Icon, { IconName, IconProps } from 'Components/Icon';
import { CalendarStatus } from 'typings/Calendar';
import styles from './LegendItem.css';

interface LegendItemProps {
  name: string;
  tooltip?: string;
  fullColorEvents: boolean;
  status?: CalendarStatus;
  icon?: IconName;
  kind?: IconProps['kind'];
}

function LegendItem(props: LegendItemProps) {
  const { name, tooltip, fullColorEvents, status, icon, kind } = props;

  return (
    <div
      className={classNames(styles.legendItem, status && styles[status])}
      title={tooltip ?? name}
    >
      <span className={styles.indicator}>
        {icon ? (
          <Icon
            className={classNames(
              styles.icon,
              fullColorEvents && styles.fullColor
            )}
            name={icon}
            kind={kind}
          />
        ) : (
          <span className={classNames(styles.bar, status && styles[status])} />
        )}
      </span>

      {name}
    </div>
  );
}

export default LegendItem;
