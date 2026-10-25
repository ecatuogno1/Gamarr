import React, { useCallback, useState } from 'react';
import SelectInput, { SelectInputOption } from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import { icons, sizes } from 'Helpers/Props';
import { Size } from 'Helpers/Props/sizes';
import NamingConfig from 'typings/Settings/NamingConfig';
import translate from 'Utilities/String/translate';
import NamingOption from './NamingOption';
import TokenCase from './TokenCase';
import TokenSeparator from './TokenSeparator';
import styles from './NamingModal.css';

interface Token {
  token: string;
  example: string;
  footNotes?: string;
  notes?: string[];
}

interface TokenGroup {
  legend: string;
  caption?: string;
  tokens: Token[];
  size?: Extract<Size, 'large'>;
  isFullFilename?: boolean;
}

function withFootNotes(
  tokens: Token[],
  notes: Record<string, string>
): Token[] {
  return tokens.map((token) =>
    token.footNotes
      ? {
          ...token,
          notes: token.footNotes
            .split(',')
            .map((id) => notes[id.trim()])
            .filter((note): note is string => note != null),
        }
      : token
  );
}

interface TokenFlags {
  game: boolean;
  additional: boolean;
}

type SeparatorInputOption = Omit<SelectInputOption, 'key'> & {
  key: TokenSeparator;
};

type CaseInputOption = Omit<SelectInputOption, 'key'> & {
  key: TokenCase;
};

const separatorOptions: SeparatorInputOption[] = [
  {
    key: ' ',
    get value() {
      return `${translate('Space')} ( )`;
    },
  },
  {
    key: '.',
    get value() {
      return `${translate('Period')} (.)`;
    },
  },
  {
    key: '_',
    get value() {
      return `${translate('Underscore')} (_)`;
    },
  },
  {
    key: '-',
    get value() {
      return `${translate('Dash')} (-)`;
    },
  },
];

const caseOptions: CaseInputOption[] = [
  {
    key: 'title',
    get value() {
      return translate('DefaultCase');
    },
  },
  {
    key: 'lower',
    get value() {
      return translate('Lowercase');
    },
  },
  {
    key: 'upper',
    get value() {
      return translate('Uppercase');
    },
  },
];

const fileNameTokens = [
  {
    token: '{Game Title} ({Release Year}){ (Region)}{ [Version]}',
    example: "Game's Title (1990) (USA, Europe) [v1.1]",
  },
  {
    token:
      '{Game Title} ({Release Year}){ (Region)}{ (Languages)}{ [Release Type]}{ [Quality Title]}',
    example: "Game's Title (1990) (USA, Europe) (En,Fr,De) [ROM-Verified]",
  },
  {
    token:
      '{Game.CleanTitle}{.Release.Year}{.Edition}{.Version}{-Release Group}',
    example: 'Games.Title.1990.Deluxe.Edition.v1.1-GAMARR',
  },
];

const gameTokens = [
  { token: '{Game Title}', example: "Game's Title", footNotes: '1' },
  { token: '{Game Title:DE}', example: 'Titel des Spiels', footNotes: '1' },
  { token: '{Game CleanTitle}', example: 'Games Title', footNotes: '1' },
  {
    token: '{Game CleanTitle:DE}',
    example: 'Titel des Spiels',
    footNotes: '1',
  },
  { token: '{Game TitleThe}', example: "Game's Title, The", footNotes: '1' },
  {
    token: '{Game CleanTitleThe}',
    example: 'Games Title, The',
    footNotes: '1',
  },
  { token: '{Game OriginalTitle}', example: 'Τίτλος ταινίας', footNotes: '1' },
  {
    token: '{Game CleanOriginalTitle}',
    example: 'Τίτλος ταινίας',
    footNotes: '1',
  },
  { token: '{Game TitleFirstCharacter}', example: 'M' },
  { token: '{Game TitleFirstCharacter:DE}', example: 'T' },
  {
    token: '{Game Collection}',
    example: 'The Game Collection',
    footNotes: '1',
  },
  {
    token: '{Game CollectionThe}',
    example: "Game's Collection, The",
    footNotes: '1',
  },
  {
    token: '{Game CleanCollectionThe}',
    example: 'Games Collection, The',
    footNotes: '1',
  },
  { token: '{Release Year}', example: '2009' },
  { token: '{Platform}', example: 'Super Nintendo Entertainment System' },
  { token: '{Platform Short}', example: 'SNES' },
  { token: '{Platform Folder}', example: 'snes' },
];

const gameIdTokens = [{ token: '{IgdbId}', example: '123456' }];

const qualityTokens = [
  { token: '{Quality Full}', example: 'ROM-Verified Proper' },
  { token: '{Quality Title}', example: 'ROM-Verified' },
];

const releaseTokens = [
  { token: '{Region}', example: 'USA, Europe' },
  { token: '{Languages}', example: 'En,Fr,De' },
  { token: '{Version}', example: 'v1.1' },
  { token: '{Release Type}', example: 'Update', footNotes: '1' },
  { token: '{Disc}', example: 'Disc 1' },
];

const releaseGroupTokens = [
  { token: '{Release Group}', example: 'Rls Grp', footNotes: '1' },
];

const editionTokens = [
  { token: '{Edition}', example: 'Deluxe Edition', footNotes: '1' },
  { token: '{Edition Tags}', example: 'Deluxe Edition', footNotes: '1' },
];

const customFormatTokens = [
  { token: '{Custom Formats}', example: 'Verified No-Intro' },
  { token: '{Custom Format:FormatName}', example: 'No-Intro' },
];

const originalTokens = [
  { token: '{Original Title}', example: 'Game.Title.USA.NSW-GAMARR' },
  { token: '{Original Filename}', example: "Game's Title (USA) (Rev 1)" },
];

function getNamingTokenGroups({ game, additional }: TokenFlags): TokenGroup[] {
  const groups: TokenGroup[] = [];

  if (game) {
    groups.push({
      legend: translate('FileNames'),
      tokens: fileNameTokens,
      size: sizes.LARGE,
      isFullFilename: true,
    });
  }

  groups.push({
    legend: translate('Game'),
    tokens: withFootNotes(gameTokens, { 1: translate('GameFootNote') }),
  });

  groups.push({ legend: translate('GameID'), tokens: gameIdTokens });

  if (additional) {
    groups.push({ legend: translate('Quality'), tokens: qualityTokens });

    groups.push({
      legend: translate('Release'),
      tokens: withFootNotes(releaseTokens, {
        1: translate('ReleaseTypeFootNote'),
      }),
    });

    groups.push({
      legend: translate('ReleaseGroup'),
      tokens: withFootNotes(releaseGroupTokens, {
        1: translate('ReleaseGroupFootNote'),
      }),
    });

    groups.push({
      legend: translate('Edition'),
      tokens: withFootNotes(editionTokens, {
        1: translate('EditionFootNote'),
      }),
    });

    groups.push({
      legend: translate('CustomFormats'),
      tokens: customFormatTokens,
    });

    groups.push({
      legend: translate('Original'),
      tokens: originalTokens,
      size: sizes.LARGE,
    });
  }

  return groups;
}

export interface NamingModalOptions {
  name: keyof Pick<NamingConfig, 'standardGameFormat' | 'gameFolderFormat'>;
  game?: boolean;
  additional?: boolean;
}

interface NamingModalProps extends NamingModalOptions {
  isOpen: boolean;
  value: string;
  onInputChange: ({ name, value }: { name: string; value: string }) => void;
  onModalClose: () => void;
}

function NamingModal(props: NamingModalProps) {
  const {
    isOpen,
    name,
    value,
    game = false,
    additional = false,
    onInputChange,
    onModalClose,
  } = props;

  const [tokenSeparator, setTokenSeparator] = useState<TokenSeparator>(' ');
  const [tokenCase, setTokenCase] = useState<TokenCase>('title');
  const [selectionStart, setSelectionStart] = useState<number | null>(null);
  const [selectionEnd, setSelectionEnd] = useState<number | null>(null);

  const handleTokenSeparatorChange = useCallback(
    ({ value }: { value: TokenSeparator }) => {
      setTokenSeparator(value);
    },
    [setTokenSeparator]
  );

  const handleTokenCaseChange = useCallback(
    ({ value }: { value: TokenCase }) => {
      setTokenCase(value);
    },
    [setTokenCase]
  );

  const handleInputSelectionChange = useCallback(
    (selectionStart: number | null, selectionEnd: number | null) => {
      setSelectionStart(selectionStart);
      setSelectionEnd(selectionEnd);
    },
    [setSelectionStart, setSelectionEnd]
  );

  const handleOptionPress = useCallback(
    ({
      isFullFilename,
      tokenValue,
    }: {
      isFullFilename: boolean;
      tokenValue: string;
    }) => {
      if (isFullFilename) {
        onInputChange({ name, value: tokenValue });
      } else if (selectionStart == null || selectionEnd == null) {
        onInputChange({
          name,
          value: `${value}${tokenValue}`,
        });
      } else {
        const start = value.substring(0, selectionStart);
        const end = value.substring(selectionEnd);
        const newValue = `${start}${tokenValue}${end}`;
        const caret = selectionStart + tokenValue.length;

        onInputChange({ name, value: newValue });

        setSelectionStart(caret);
        setSelectionEnd(caret);
      }
    },
    [name, value, selectionEnd, selectionStart, onInputChange]
  );

  const tokenGroups = getNamingTokenGroups({ game, additional });

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {game ? translate('FileNameTokens') : translate('FolderNameTokens')}
        </ModalHeader>

        <ModalBody>
          <div className={styles.builder}>
            <TextInput
              className={styles.formatInput}
              name={name}
              value={value}
              onChange={onInputChange}
              onSelectionChange={handleInputSelectionChange}
            />

            <div className={styles.controls}>
              <div className={styles.control}>
                <span className={styles.controlLabel}>
                  {translate('Separator')}
                </span>

                <SelectInput
                  className={styles.namingSelect}
                  name="separator"
                  value={tokenSeparator}
                  values={separatorOptions}
                  onChange={handleTokenSeparatorChange}
                />
              </div>

              <div className={styles.control}>
                <span className={styles.controlLabel}>
                  {translate('Capitalization')}
                </span>

                <SelectInput
                  className={styles.namingSelect}
                  name="case"
                  value={tokenCase}
                  values={caseOptions}
                  onChange={handleTokenCaseChange}
                />
              </div>
            </div>
          </div>

          <div className={styles.instruction}>
            <Icon name={icons.INFO} size={14} />
            <span>{translate('NamingTokenSelectHelpText')}</span>
          </div>

          {tokenGroups.map((group) => (
            <ModalSection key={group.legend} title={group.legend}>
              {group.caption ? (
                <p className={styles.caption}>{group.caption}</p>
              ) : null}

              <div className={styles.groups}>
                {group.tokens.map(({ token, example, notes }) => (
                  <NamingOption
                    key={token}
                    token={token}
                    example={example}
                    notes={notes}
                    isFullFilename={group.isFullFilename}
                    size={group.size}
                    tokenSeparator={tokenSeparator}
                    tokenCase={tokenCase}
                    onPress={handleOptionPress}
                  />
                ))}
              </div>
            </ModalSection>
          ))}
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>{translate('Close')}</Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default NamingModal;
