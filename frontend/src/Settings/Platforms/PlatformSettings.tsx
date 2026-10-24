import React, { useCallback, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import CheckInput from 'Components/Form/CheckInput';
import TextInput from 'Components/Form/TextInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableRow from 'Components/Table/TableRow';
import { kinds } from 'Helpers/Props';
import SettingsToolbar from 'Settings/SettingsToolbar';
import { savePlatformEnabled } from 'Store/Actions/settingsActions';
import { CheckInputChanged, InputChanged } from 'typings/inputs';
import Platform, { PlatformFamily } from 'typings/Platform';
import translate from 'Utilities/String/translate';
import styles from './PlatformSettings.css';

const familyOrder: PlatformFamily[] = [
  'pc',
  'nintendo',
  'sony',
  'microsoft',
  'sega',
  'nec',
  'snk',
  'atari',
  'arcade',
  'other',
];

function getFamilyName(family: PlatformFamily) {
  switch (family) {
    case 'pc':
      return translate('PlatformFamilyPc');
    case 'nintendo':
      return 'Nintendo';
    case 'sony':
      return 'Sony';
    case 'microsoft':
      return 'Microsoft';
    case 'sega':
      return 'Sega';
    case 'nec':
      return 'NEC';
    case 'snk':
      return 'SNK';
    case 'atari':
      return 'Atari';
    case 'arcade':
      return translate('PlatformFamilyArcade');
    default:
      return translate('Other');
  }
}

const columns: Column[] = [
  {
    name: 'enabled',
    label: () => translate('Enabled'),
    isVisible: true,
  },
  {
    name: 'name',
    label: () => translate('Name'),
    isVisible: true,
  },
  {
    name: 'shortName',
    label: () => translate('ShortName'),
    isVisible: true,
  },
  {
    name: 'folderName',
    label: () => translate('FolderName'),
    isVisible: true,
  },
  {
    name: 'fileExtensions',
    label: () => translate('FileExtensions'),
    isVisible: true,
  },
  {
    name: 'newznabCategories',
    label: () => translate('Categories'),
    isVisible: true,
  },
];

interface PlatformRowProps {
  platform: Platform;
  onEnabledChange: (id: number, enabled: boolean) => void;
}

function PlatformRow({ platform, onEnabledChange }: PlatformRowProps) {
  const {
    id,
    name,
    shortName,
    folderName,
    fileExtensions,
    newznabCategories,
    enabled,
  } = platform;

  const handleChange = useCallback(
    ({ value }: CheckInputChanged) => {
      onEnabledChange(id, value);
    },
    [id, onEnabledChange]
  );

  return (
    <TableRow className={enabled ? undefined : styles.disabled}>
      <TableRowCell className={styles.enabled}>
        <CheckInput
          name={`platform-${id}`}
          value={enabled}
          onChange={handleChange}
        />
      </TableRowCell>
      <TableRowCell>{name}</TableRowCell>
      <TableRowCell>{shortName}</TableRowCell>
      <TableRowCell className={styles.code}>{folderName}</TableRowCell>
      <TableRowCell className={styles.extensions}>
        {fileExtensions.join(' ')}
      </TableRowCell>
      <TableRowCell>{newznabCategories.join(', ')}</TableRowCell>
    </TableRow>
  );
}

// Platforms are seeded reference data, only Enabled can be changed. Disabled platforms aren't offered when adding
// games and their releases aren't looked up.
function PlatformSettings() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, error, items } = useSelector(
    (state: AppState) => state.settings.platforms
  );

  const [filter, setFilter] = useState('');

  const handleFilterChange = useCallback(({ value }: InputChanged<string>) => {
    setFilter(value);
  }, []);

  const handleEnabledChange = useCallback(
    (id: number, enabled: boolean) => {
      dispatch(savePlatformEnabled({ id, enabled }));
    },
    [dispatch]
  );

  const groups = useMemo(() => {
    const term = filter.trim().toLowerCase();

    const filtered = items.filter(
      (p) =>
        !term ||
        p.name.toLowerCase().includes(term) ||
        p.shortName.toLowerCase().includes(term) ||
        p.folderName.toLowerCase().includes(term)
    );

    return familyOrder
      .map((family) => ({
        family,
        platforms: filtered
          .filter((p) => p.family === family)
          .sort(
            (a, b) =>
              a.generation - b.generation || a.name.localeCompare(b.name)
          ),
      }))
      .filter((group) => group.platforms.length);
  }, [items, filter]);

  const enabledCount = items.filter((p) => p.enabled).length;

  return (
    <PageContent title={translate('Platforms')}>
      <SettingsToolbar showSave={false} />

      <PageContentBody>
        <Alert kind={kinds.INFO}>
          {translate('PlatformSettingsSummary', {
            enabledCount,
            count: items.length,
          })}
        </Alert>

        <div className={styles.filter}>
          <TextInput
            name="platformFilter"
            value={filter}
            placeholder={translate('PlatformFilterPlaceholder')}
            onChange={handleFilterChange}
          />
        </div>

        {isFetching && !isPopulated ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>{translate('PlatformsLoadError')}</Alert>
        ) : null}

        {isPopulated
          ? groups.map(({ family, platforms }) => (
              <FieldSet key={family} legend={getFamilyName(family)}>
                <Table columns={columns}>
                  <TableBody>
                    {platforms.map((platform) => (
                      <PlatformRow
                        key={platform.id}
                        platform={platform}
                        onEnabledChange={handleEnabledChange}
                      />
                    ))}
                  </TableBody>
                </Table>
              </FieldSet>
            ))
          : null}
      </PageContentBody>
    </PageContent>
  );
}

export default PlatformSettings;
