import React from 'react';
import Link from 'Components/Link/Link';
import { Image } from 'Game/Game';
import translate from 'Utilities/String/translate';
import styles from './GameScreenshots.css';

interface GameScreenshotsProps {
  screenshots: Image[];
}

// IGDB screenshots are stored as 1080p images, the strip uses IGDB's smaller size
function getThumbnailUrl(image: Image) {
  const url = image.remoteUrl ?? image.url;

  return url.replace(/\/t_[a-z0-9_]+\//, '/t_screenshot_med/');
}

function GameScreenshots({ screenshots }: GameScreenshotsProps) {
  return (
    <div className={styles.screenshots}>
      {screenshots.map((image, index) => (
        <Link
          key={image.remoteUrl ?? image.url}
          className={styles.screenshot}
          to={image.remoteUrl ?? image.url}
        >
          <img
            className={styles.image}
            src={getThumbnailUrl(image)}
            alt={translate('ScreenshotNumber', { number: index + 1 })}
            loading="lazy"
          />
        </Link>
      ))}
    </div>
  );
}

export default GameScreenshots;
