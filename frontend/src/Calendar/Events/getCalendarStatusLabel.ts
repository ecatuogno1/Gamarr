import { CalendarStatus } from 'typings/Calendar';
import translate from 'Utilities/String/translate';

const statusTranslationKeys: Record<CalendarStatus, string> = {
  downloaded: 'DownloadedAndMonitored',
  queue: 'Queued',
  unmonitored: 'DownloadedButNotMonitored',
  missingMonitored: 'MissingMonitoredAndConsideredAvailable',
  missingUnmonitored: 'MissingNotMonitored',
  continuing: 'Unreleased',
};

function getCalendarStatusLabel(status: CalendarStatus) {
  return translate(statusTranslationKeys[status]);
}

export default getCalendarStatusLabel;
