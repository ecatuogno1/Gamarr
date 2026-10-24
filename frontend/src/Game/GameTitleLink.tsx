import React from 'react';
import Link, { LinkProps } from 'Components/Link/Link';
import { GamePlatform } from 'Game/Game';
import { getTitleWithPlatform } from 'Game/Title/gameTitle';

interface GameTitleLinkProps extends LinkProps {
  titleSlug: string;
  title: string;
  year?: number;
  // Shown as "Title (SNES)", the link opens the title page with the platform selected
  platform?: Pick<GamePlatform, 'name' | 'shortName'> | null;
}

function GameTitleLink({
  titleSlug,
  title,
  year = 0,
  platform,
  ...otherProps
}: GameTitleLinkProps) {
  const link = `/game/${titleSlug}`;
  const yearText = year > 0 ? ` (${year})` : '';

  return (
    <Link
      to={link}
      title={platform ? `${title}${yearText} - ${platform.name}` : title}
      {...otherProps}
    >
      {getTitleWithPlatform(`${title}${yearText}`, platform)}
    </Link>
  );
}

export default GameTitleLink;
