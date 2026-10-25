import { Action, Location, UnregisterCallback } from 'history';
import { useCallback, useEffect, useRef, useState } from 'react';
import { useHistory } from 'react-router';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';

// Sonarr v5 uses react-router 7's useBlocker here. Gamarr is still on
// react-router 5, so navigation is blocked with history.block instead.

interface UseSettingsSaveOptions {
  hasPendingChanges?: boolean;
  onSavePress?: () => void;
}

interface PendingChangesModalProps {
  isOpen: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

interface UseSettingsSaveResult {
  pendingChangesModalProps: PendingChangesModalProps;
}

export default function useSettingsSave({
  hasPendingChanges = false,
  onSavePress,
}: UseSettingsSaveOptions): UseSettingsSaveResult {
  const { bindShortcut, unbindShortcut } = useKeyboardShortcuts();
  const history = useHistory();
  const [nextLocation, setNextLocation] = useState<Location | null>(null);
  const [nextLocationAction, setNextLocationAction] = useState<Action | null>(
    null
  );
  const hasConfirmed = useRef(false);
  const unblocker = useRef<UnregisterCallback>();

  const handleConfirmNavigation = useCallback(() => {
    if (!nextLocation) {
      return;
    }

    const path = `${nextLocation.pathname}${nextLocation.search}`;

    hasConfirmed.current = true;

    if (nextLocationAction === 'PUSH') {
      history.push(path);
    } else {
      // Back and forward both use POP, so the direction is unknown;
      // assume back.
      history.goBack();
    }
  }, [nextLocation, nextLocationAction, history]);

  const handleCancelNavigation = useCallback(() => {
    setNextLocation(null);
    setNextLocationAction(null);
    hasConfirmed.current = false;
  }, []);

  const handleRouterLeaving = useCallback(
    (routerLocation: Location, routerAction: Action) => {
      if (hasConfirmed.current) {
        setNextLocation(null);
        setNextLocationAction(null);
        hasConfirmed.current = false;

        return;
      }

      if (hasPendingChanges) {
        setNextLocation(routerLocation);
        setNextLocationAction(routerAction);

        return false;
      }

      return;
    },
    [hasPendingChanges]
  );

  useEffect(() => {
    unblocker.current = history.block(handleRouterLeaving);

    return () => {
      unblocker.current?.();
    };
  }, [history, handleRouterLeaving]);

  useEffect(() => {
    bindShortcut(
      'saveSettings',
      (e) => {
        e.preventDefault();

        if (hasPendingChanges) {
          onSavePress?.();
        }
      },
      { isGlobal: true }
    );

    return () => {
      unbindShortcut('saveSettings');
    };
  }, [hasPendingChanges, bindShortcut, unbindShortcut, onSavePress]);

  return {
    pendingChangesModalProps: {
      isOpen: nextLocation !== null,
      onConfirm: handleConfirmNavigation,
      onCancel: handleCancelNavigation,
    },
  };
}
