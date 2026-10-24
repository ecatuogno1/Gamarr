import React from 'react';
import GameImage, { GameImageProps } from './GameImage';

// A faint game controller on the dark poster background (168x247, like a 3:4 cover)
const posterPlaceholderSvg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 168 247">
<rect width="168" height="247" fill="#2f2f2f"/>
<g fill="#3a3a3a" transform="translate(14 54) scale(0.274)">
<rect x="72" y="146" width="368" height="178" rx="89"/>
<ellipse cx="138" cy="302" rx="72" ry="100" transform="rotate(24 138 302)"/>
<ellipse cx="374" cy="302" rx="72" ry="100" transform="rotate(-24 374 302)"/>
</g>
<g fill="#2f2f2f" transform="translate(14 54) scale(0.274)">
<rect x="148" y="190" width="34" height="96" rx="7"/>
<rect x="117" y="221" width="96" height="34" rx="7"/>
<circle cx="352" cy="200" r="19"/>
<circle cx="392" cy="238" r="19"/>
<circle cx="312" cy="238" r="19"/>
<circle cx="352" cy="276" r="19"/>
</g>
</svg>`;

const posterPlaceholder = `data:image/svg+xml;charset=utf-8,${encodeURIComponent(
  posterPlaceholderSvg
)}`;

interface GamePosterProps
  extends Omit<GameImageProps, 'coverType' | 'placeholder'> {
  size?: 250 | 500;
}

function GamePoster({ size = 250, ...otherProps }: GamePosterProps) {
  return (
    <GameImage
      {...otherProps}
      size={size}
      coverType="poster"
      placeholder={posterPlaceholder}
    />
  );
}

export default GamePoster;
