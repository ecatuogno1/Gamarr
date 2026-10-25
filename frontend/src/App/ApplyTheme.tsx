import { useLayoutEffect } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import useTheme from 'Helpers/Hooks/useTheme';

function ApplyTheme() {
  const theme = useTheme();
  const { enableColorImpairedMode, headerColor } = useSelector(
    (state: AppState) => state.settings.ui.item
  );

  useLayoutEffect(() => {
    document.documentElement.dataset.theme = theme;
    document.documentElement.style.removeProperty('--pageBackground');
  }, [theme]);

  useLayoutEffect(() => {
    if (enableColorImpairedMode) {
      document.documentElement.dataset.colorImpaired = '';
    } else {
      delete document.documentElement.dataset.colorImpaired;
    }
  }, [enableColorImpairedMode]);

  useLayoutEffect(() => {
    if (headerColor === 'gradient') {
      document.documentElement.dataset.headerColor = headerColor;
    } else {
      delete document.documentElement.dataset.headerColor;
    }
  }, [headerColor]);

  return null;
}

export default ApplyTheme;
