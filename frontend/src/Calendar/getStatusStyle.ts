import { CalendarStatus } from 'typings/Calendar';

function getStatusStyle(
  hasFile: boolean,
  downloading: boolean,
  isMonitored: boolean,
  isAvailable: boolean
): CalendarStatus {
  if (downloading) {
    return 'queue';
  }

  if (hasFile && isMonitored) {
    return 'downloaded';
  }

  if (hasFile && !isMonitored) {
    return 'unmonitored';
  }

  if (isAvailable && isMonitored) {
    return 'missingMonitored';
  }

  if (!isMonitored) {
    return 'missingUnmonitored';
  }

  return 'continuing';
}

export default getStatusStyle;
