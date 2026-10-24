import React, { useCallback } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import { CheckInputChanged } from 'typings/inputs';
import Platform from 'typings/Platform';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import styles from './PlatformCheckList.css';

export interface PlatformCheckListOption {
  id: number;
  name: string;
  shortName: string;
  folderName: string;
  // Already in the library on this platform
  isOwned: boolean;
}

// Enabled platforms IGDB lists for a title (Gamarr ids), or every enabled platform when IGDB lists none
export function getPlatformOptions(
  platforms: Platform[],
  availablePlatformIds: number[],
  ownedPlatformIds: number[]
): PlatformCheckListOption[] {
  const enabled = platforms.filter((p) => p.enabled);
  const available = enabled.filter((p) => availablePlatformIds.includes(p.id));

  return (available.length ? available : enabled)
    .concat(
      platforms.filter(
        (p) =>
          ownedPlatformIds.includes(p.id) &&
          !availablePlatformIds.includes(p.id)
      )
    )
    .filter((p, index, all) => all.findIndex((o) => o.id === p.id) === index)
    .sort(sortByProp('name'))
    .map((p) => ({
      id: p.id,
      name: p.name,
      shortName: p.shortName,
      folderName: p.folderName,
      isOwned: ownedPlatformIds.includes(p.id),
    }));
}

// The lookup folder is built for the lookup platform ("{Platform Folder}/{Title} ({Year})" by default),
// swap its platform folder for the folder of the platform the version is added on
export function getPlatformGameFolder(
  folder: string,
  options: PlatformCheckListOption[],
  platformId: number
) {
  const platform = options.find((o) => o.id === platformId);
  const segments = folder.split(/[\\/]/);

  if (
    !platform?.folderName ||
    segments.length < 2 ||
    !options.some(
      (o) => o.folderName?.toLowerCase() === segments[0].toLowerCase()
    )
  ) {
    return folder;
  }

  const separator = folder.charAt(segments[0].length);

  return [platform.folderName, ...segments.slice(1)].join(separator);
}

export function getPlatformIdsFromIgdbIds(
  platforms: Platform[],
  igdbPlatformIds: number[] = []
) {
  return platforms
    .filter((p) => igdbPlatformIds.includes(p.igdbPlatformId))
    .map((p) => p.id);
}

interface PlatformCheckListProps {
  options: PlatformCheckListOption[];
  selectedIds: number[];
  onChange: (selectedIds: number[]) => void;
}

function PlatformCheckList({
  options,
  selectedIds,
  onChange,
}: PlatformCheckListProps) {
  const handleChange = useCallback(
    ({ name, value }: CheckInputChanged) => {
      const id = parseInt(name.replace('platform-', ''));

      onChange(
        value
          ? [...selectedIds, id]
          : selectedIds.filter((selectedId) => selectedId !== id)
      );
    },
    [selectedIds, onChange]
  );

  return (
    <div className={styles.platforms}>
      {options.map((option) => (
        <label
          key={option.id}
          className={option.isOwned ? styles.owned : styles.platform}
          title={
            option.isOwned
              ? translate('GameIsInLibraryOnPlatform', {
                  platform: option.name,
                })
              : option.name
          }
        >
          <CheckInput
            containerClassName={styles.checkContainer}
            name={`platform-${option.id}`}
            value={option.isOwned || selectedIds.includes(option.id)}
            isDisabled={option.isOwned}
            onChange={handleChange}
          />

          <span className={styles.name}>{option.name}</span>

          <span className={styles.hint}>
            {option.isOwned ? translate('InLibrary') : option.shortName}
          </span>
        </label>
      ))}
    </div>
  );
}

export default PlatformCheckList;
