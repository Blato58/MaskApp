# Icon Sources

MaskApp Pages shortcut icons use a curated offline icon catalog so the add-item
screen can offer fast local previews without runtime internet access.

## Sources

- Lucide icons: ISC license, https://lucide.dev/license
- Material Symbols / Material Design Icons: Apache License 2.0,
  https://github.com/google/material-design-icons
- Phosphor Icons: MIT license, https://phosphoricons.com/

## Current Integration

- The app stores icon-pack metadata in `GalleryIconOption`.
- SVG previews are vendored under
  `src/MaskApp.App/Resources/Images/page-icons/` for offline use.
- Shortcut layouts persist only stable icon keys, display labels, and colors;
  the saved data does not depend on the asset file format.

## Curated Packs

- Mask: text, face, animation, rave, favorite, safe, pack.
- Lucide: message, smile, laugh, neutral, heart, zap, music, radio, mic, eye,
  star, flame, moon, sun, party, palette.
- Material: wave, pets, excited, bolt, favorite, visibility, equalizer, comedy,
  celebration, volume, light mode, dark mode.
- Phosphor: cat, dog, alien, happy mask, music notes, lightning, fire, skull,
  smiley, eyes, disco, sparkle.
