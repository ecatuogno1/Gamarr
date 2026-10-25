import {
  autoUpdate,
  flip,
  FloatingPortal,
  shift,
  size,
  useDismiss,
  useFloating,
  useInteractions,
} from '@floating-ui/react';
import React, {
  KeyboardEvent,
  MouseEvent,
  useCallback,
  useEffect,
  useId,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { icons, kinds } from 'Helpers/Props';
import {
  queueLookupGame,
  setImportGameValue,
} from 'Store/Actions/importGameActions';
import createAllGamesSelector from 'Store/Selectors/createAllGamesSelector';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { ImportGameAppState } from '../importGameTypes';
import ImportGameSearchResult from './ImportGameSearchResult';
import styles from './ImportGameSelectGame.css';

const DROPDOWN_MIN_WIDTH = 360;
const DROPDOWN_VIEWPORT_MARGIN = 12;
const LOOKUP_DELAY = 200;

function handleResultsMouseDown(event: MouseEvent<HTMLDivElement>) {
  event.preventDefault();
}

interface ImportGameSelectGameProps {
  id: string;
  onEditingChange: (isEditing: boolean) => void;
}

function ImportGameSelectGame({
  id,
  onEditingChange,
}: ImportGameSelectGameProps) {
  const dispatch = useDispatch();

  const item = useSelector((state: ImportGameAppState) =>
    state.importGame.items.find((i) => i.id === id)
  );
  const isLookingUpGame = useSelector(
    (state: ImportGameAppState) => state.importGame.isLookingUpGame
  );
  const allGames = useSelector(createAllGamesSelector());

  const {
    term = id,
    selectedGame,
    items = [],
    isFetching = true,
    isPopulated = false,
    isQueued = true,
    error,
  } = item ?? {};

  const existingIgdbIds = useMemo(() => {
    return new Set(allGames.map((game) => game.igdbId));
  }, [allGames]);

  const isExistingGame =
    !!selectedGame && existingIgdbIds.has(selectedGame.igdbId);

  const [editText, setEditText] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [highlightedIndex, setHighlightedIndex] = useState(0);

  const inputRef = useRef<HTMLInputElement>(null);
  const lookupTimeoutRef = useRef<ReturnType<typeof setTimeout>>();
  const resultsId = useId();

  const isSearching = isFetching || (isLookingUpGame && isQueued);
  const isOpen = isEditing && items.length > 0;
  const errorMessage = error?.responseJSON?.message;

  const { refs, context, floatingStyles } = useFloating({
    middleware: [
      flip({ crossAxis: false, mainAxis: true }),
      shift({ padding: DROPDOWN_VIEWPORT_MARGIN }),
      size({
        apply({ rects, elements }) {
          const maxWidth =
            document.documentElement.clientWidth - DROPDOWN_VIEWPORT_MARGIN * 2;

          elements.floating.style.width = `${Math.min(
            Math.max(rects.reference.width, DROPDOWN_MIN_WIDTH),
            maxWidth
          )}px`;
        },
      }),
    ],
    open: isOpen,
    placement: 'bottom-start',
    whileElementsMounted: autoUpdate,
    onOpenChange: (open) => {
      if (!open) {
        setIsEditing(false);
      }
    },
  });

  const dismiss = useDismiss(context);
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss]);

  const handleFindGamePress = useCallback(() => {
    setEditText(term);
    setIsEditing(true);
  }, [term]);

  const handleBlur = useCallback(() => {
    setIsEditing(false);
  }, []);

  const handleInputChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setEditText(value);
      setHighlightedIndex(0);

      if (lookupTimeoutRef.current) {
        clearTimeout(lookupTimeoutRef.current);
      }

      lookupTimeoutRef.current = setTimeout(() => {
        dispatch(
          queueLookupGame({
            name: id,
            term: value,
            topOfQueue: true,
          })
        );
      }, LOOKUP_DELAY);
    },
    [id, dispatch]
  );

  const handleGameSelect = useCallback(
    (igdbId: number) => {
      const nextGame = items.find((m) => m.igdbId === igdbId);

      if (!nextGame) {
        return;
      }

      // @ts-expect-error actions aren't typed
      dispatch(setImportGameValue({ id, selectedGame: nextGame }));
      setIsEditing(false);
      inputRef.current?.blur();
    },
    [id, items, dispatch]
  );

  const handleKeyDown = useCallback(
    (event: KeyboardEvent<HTMLDivElement>) => {
      if (event.key === 'ArrowDown') {
        event.preventDefault();
        setHighlightedIndex((index) => Math.min(index + 1, items.length - 1));
      } else if (event.key === 'ArrowUp') {
        event.preventDefault();
        setHighlightedIndex((index) => Math.max(index - 1, 0));
      } else if (event.key === 'Enter') {
        const result = items[highlightedIndex];

        if (result) {
          event.preventDefault();
          handleGameSelect(result.igdbId);
        }
      } else if (event.key === 'Escape') {
        setIsEditing(false);
        inputRef.current?.blur();
      }
    },
    [items, highlightedIndex, handleGameSelect]
  );

  useEffect(() => {
    onEditingChange(isEditing);

    return () => {
      onEditingChange(false);
    };
  }, [isEditing, onEditingChange]);

  useEffect(() => {
    setHighlightedIndex(0);
  }, [items]);

  useEffect(() => {
    if (isEditing) {
      inputRef.current?.focus();
      inputRef.current?.select();
    }
  }, [isEditing]);

  useEffect(() => {
    return () => {
      if (lookupTimeoutRef.current) {
        clearTimeout(lookupTimeoutRef.current);
      }
    };
  }, []);

  const activeRowId =
    isOpen && items.length > 0 ? `${resultsId}_${highlightedIndex}` : undefined;

  const hasWarning =
    !!error || (isPopulated && !selectedGame) || isExistingGame;

  let warningTitle = translate('Existing');

  if (error) {
    warningTitle = errorMessage ?? translate('SearchFailedPleaseTryAgainLater');
  } else if (isPopulated && !selectedGame) {
    warningTitle = translate('NoMatchFound');
  }

  let gameControl: React.ReactNode = null;

  if (isEditing) {
    gameControl = (
      <div
        ref={refs.setReference}
        className={styles.field}
        role="combobox"
        aria-expanded={isOpen}
        aria-haspopup="grid"
        aria-controls={isOpen ? resultsId : undefined}
        aria-activedescendant={activeRowId}
        onKeyDown={handleKeyDown}
        {...getReferenceProps()}
      >
        <Icon className={styles.searchIcon} name={icons.SEARCH} />

        <TextInput
          ref={inputRef}
          className={styles.input}
          name={`${id}_game`}
          value={editText}
          placeholder={translate('SearchForGame')}
          onChange={handleInputChange}
          onBlur={handleBlur}
        />

        <div className={styles.status}>
          {isSearching ? (
            <LoadingIndicator className={styles.loadingIndicator} size={20} />
          ) : null}

          {!isSearching && hasWarning ? (
            <Icon
              name={icons.WARNING}
              kind={kinds.WARNING}
              title={warningTitle}
            />
          ) : null}
        </div>
      </div>
    );
  } else if (selectedGame) {
    const { title, year } = selectedGame;
    const hasYearInTitle = title.includes(String(year));

    gameControl = (
      <Link
        className={styles.matchedGame}
        title={translate('ChangeMatch')}
        onPress={handleFindGamePress}
      >
        <span className={styles.matchedTitle}>{title}</span>

        {year > 0 && !hasYearInTitle ? (
          <span className={styles.matchedYear}>({year})</span>
        ) : null}

        <Icon className={styles.matchedIcon} name={icons.SEARCH} size={13} />
      </Link>
    );
  } else if (isSearching && !isPopulated) {
    gameControl = (
      <LoadingIndicator className={styles.loadingIndicator} size={20} />
    );
  } else {
    gameControl = (
      <Button
        kind={kinds.DEFAULT}
        title={error ? warningTitle : undefined}
        onPress={handleFindGamePress}
      >
        {translate('FindGame')}
      </Button>
    );
  }

  return (
    <>
      {gameControl}

      {isOpen ? (
        <FloatingPortal id="portal-root">
          <div
            ref={refs.setFloating}
            className={styles.contentContainer}
            style={floatingStyles}
            {...getFloatingProps()}
          >
            <div
              className={styles.results}
              role="grid"
              id={resultsId}
              onMouseDown={handleResultsMouseDown}
            >
              {items.map((result, index) => {
                return (
                  <ImportGameSearchResult
                    key={result.igdbId}
                    id={`${resultsId}_${index}`}
                    igdbId={result.igdbId}
                    title={result.title}
                    year={result.year}
                    platform={result.platform?.shortName}
                    isExistingGame={existingIgdbIds.has(result.igdbId)}
                    isHighlighted={index === highlightedIndex}
                    onPress={handleGameSelect}
                  />
                );
              })}
            </div>
          </div>
        </FloatingPortal>
      ) : null}
    </>
  );
}

export default ImportGameSelectGame;
